import { database } from "../cosmos-client.js";
import { UserDto } from "../dtos/UserDto.js";

/**
 * Creates a UserDto from a Cosmos DB document
 * @param {Object} document - Raw document from Cosmos DB
 * @returns {UserDto} - Formatted UserDto instance
 */
function fromCosmosDocument(document) {
  if (!document) return null;

  return new UserDto({
    id: document.id,
    username: document.username,
    email: document.email,
    passwordHash: document.passwordHash,
    isTemporaryPassword: document.isTemporaryPassword || false,
    registeredAt: document.registeredAt
      ? new Date(document.registeredAt)
      : new Date(),
    lastUpdatedAt: document.lastUpdatedAt
      ? new Date(document.lastUpdatedAt)
      : null,
    lastLoginAt: document.lastLoginAt ? new Date(document.lastLoginAt) : null,
    isActive: document.isActive !== undefined ? document.isActive : true,
    role: document.role || "User",
    profileImageUrl: document.profileImageUrl,
    following: document.following || [],
  });
}

/**
 * ProfileAPI - Data source for user profile operations
 * Handles following/unfollowing users in Cosmos DB
 */
export class ProfileAPI {
  constructor() {
    this.container = database.container("users");
  }

  /**
   * Gets a user by their ID
   * @param {string} userId - The user ID
   * @returns {Promise<UserDto|null>} - The user or null if not found
   */
  async getUserById(userId) {
    try {
      const { resource } = await this.container.item(userId, userId).read();
      return resource ? fromCosmosDocument(resource) : null;
    } catch (error) {
      if (error.code === 404) {
        return null;
      }
      console.error("Error fetching user by ID:", error);
      throw new Error("Failed to fetch user");
    }
  }

  /**
   * Follows a user by adding their ID to the current user's following list
   * @param {string} currentUserId - The ID of the user who wants to follow
   * @param {string} targetUserId - The ID of the user to follow
   * @returns {Promise<Object>} - Success message with updated following list
   */
  async followUser(currentUserId, targetUserId) {
    try {
      // Validate that users can't follow themselves
      if (currentUserId === targetUserId) {
        throw new Error("Users cannot follow themselves");
      }

      // Get the current user
      const currentUser = await this.getUserById(currentUserId);
      if (!currentUser) {
        throw new Error("Current user not found");
      }

      // Verify target user exists
      const targetUser = await this.getUserById(targetUserId);
      if (!targetUser) {
        throw new Error("Target user not found");
      }

      // Initialize following array if it doesn't exist
      if (!currentUser.following) {
        currentUser.following = [];
      }

      // Check if already following
      if (currentUser.following.includes(targetUserId)) {
        return {
          success: false,
          message: "Already following this user",
          following: currentUser.following,
        };
      }

      // Add target user to following list
      currentUser.following.push(targetUserId);
      currentUser.lastUpdatedAt = new Date();

      // Update user in Cosmos DB
      const { resource } = await this.container.items.upsert(currentUser);

      return {
        success: true,
        message: "Successfully followed user",
        following: resource.following,
      };
    } catch (error) {
      console.error("Error following user:", error);
      throw error;
    }
  }

  /**
   * Unfollows a user by removing their ID from the current user's following list
   * @param {string} currentUserId - The ID of the user who wants to unfollow
   * @param {string} targetUserId - The ID of the user to unfollow
   * @returns {Promise<Object>} - Success message with updated following list
   */
  async unfollowUser(currentUserId, targetUserId) {
    try {
      // Get the current user
      const currentUser = await this.getUserById(currentUserId);
      if (!currentUser) {
        throw new Error("Current user not found");
      }

      // Initialize following array if it doesn't exist
      if (!currentUser.following) {
        currentUser.following = [];
      }

      // Check if not following
      if (!currentUser.following.includes(targetUserId)) {
        return {
          success: false,
          message: "Not following this user",
          following: currentUser.following,
        };
      }

      // Remove target user from following list
      currentUser.following = currentUser.following.filter(
        (id) => id !== targetUserId
      );
      currentUser.lastUpdatedAt = new Date();

      // Update user in Cosmos DB
      const { resource } = await this.container.items.upsert(currentUser);

      return {
        success: true,
        message: "Successfully unfollowed user",
        following: resource.following,
      };
    } catch (error) {
      console.error("Error unfollowing user:", error);
      throw error;
    }
  }

  /**
   * Gets the list of users that a user is following
   * @param {string} userId - The user ID
   * @returns {Promise<UserDto[]>} - Array of users being followed
   */
  async getFollowing(userId) {
    try {
      const user = await this.getUserById(userId);
      if (!user || !user.following || user.following.length === 0) {
        return [];
      }

      // Fetch all followed users
      const followingPromises = user.following.map((followedId) =>
        this.getUserById(followedId)
      );
      const followingUsers = await Promise.all(followingPromises);

      // Filter out any null results (users that don't exist anymore)
      return followingUsers.filter((u) => u !== null);
    } catch (error) {
      console.error("Error fetching following list:", error);
      throw new Error("Failed to fetch following list");
    }
  }

  /**
   * Gets the list of users that follow a specific user
   * @param {string} userId - The user ID
   * @returns {Promise<UserDto[]>} - Array of followers
   */
  async getFollowers(userId) {
    try {
      const query = {
        query:
          "SELECT * FROM c WHERE ARRAY_CONTAINS(c.following, @userId) AND c.isActive = true",
        parameters: [{ name: "@userId", value: userId }],
      };

      const { resources } = await this.container.items.query(query).fetchAll();
      return resources.map((doc) => fromCosmosDocument(doc));
    } catch (error) {
      console.error("Error fetching followers:", error);
      throw new Error("Failed to fetch followers");
    }
  }

  /**
   * Gets the follower count for a user
   * @param {string} userId - The user ID
   * @returns {Promise<number>} - Number of followers
   */
  async getFollowerCount(userId) {
    try {
      const followers = await this.getFollowers(userId);
      return followers.length;
    } catch (error) {
      console.error("Error getting follower count:", error);
      return 0;
    }
  }

  /**
   * Gets the following count for a user
   * @param {string} userId - The user ID
   * @returns {Promise<number>} - Number of users being followed
   */
  async getFollowingCount(userId) {
    try {
      const user = await this.getUserById(userId);
      return user && user.following ? user.following.length : 0;
    } catch (error) {
      console.error("Error getting following count:", error);
      return 0;
    }
  }
}

// Export singleton instance
export const profileAPI = new ProfileAPI();
