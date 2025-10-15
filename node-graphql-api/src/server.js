import { ApolloServer } from 'apollo-server';
import typeDefs from './schema.js';
import * as resolvers from './resolvers.js';
import dotenv from 'dotenv';

dotenv.config();

const server = new ApolloServer({
  typeDefs,
  resolvers,
  context: () => ({}) // Add any context/dataSources here if needed
});

server.listen({ port: process.env.PORT || 4000 }).then(({ url }) => {
  console.log(`🚀 Apollo Server ready at ${url}`);
}).catch(err => {
    console.error('Apollo Server failed to start:', err);
  });