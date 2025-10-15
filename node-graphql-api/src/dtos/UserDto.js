/**
 * UserDto - Data Transfer Object for User
 * Matches the .NET UserDto structure in SocialMedia.Api
 */
export class UserDto {
  constructor({
    id = null,
    username = null,
    email = null,
    passwordHash = null,
    isTemporaryPassword = false,
    registeredAt = new Date(),
    lastUpdatedAt = null,
    lastLoginAt = null,
    isActive = true,
    role = 'User',
    profileImageUrl = null,
    following = []
  } = {}) {
    this.id = id;
    this.username = username;
    this.email = email;
    this.passwordHash = passwordHash;
    this.isTemporaryPassword = isTemporaryPassword;
    this.registeredAt = registeredAt;
    this.lastUpdatedAt = lastUpdatedAt;
    this.lastLoginAt = lastLoginAt;
    this.isActive = isActive;
    this.role = role;
    this.profileImageUrl = profileImageUrl;
    this.following = following || [];
  }
}
