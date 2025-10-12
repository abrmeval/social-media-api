require('dotenv').config();
const { ApolloServer } = require('apollo-server');
const typeDefs = require('./schema');
const resolvers = require('./resolvers');

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