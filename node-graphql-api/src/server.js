const express = require('express');
const { graphqlHTTP } = require('express-graphql');
const schema = require('./schema');
require('dotenv').config();

const app = express();
app.use('/graphql', graphqlHTTP({
  schema,
  graphiql: true // Enables GraphQL Playground
}));

const PORT = process.env.PORT || 4000;
app.listen(PORT, () => {
  console.log(`GraphQL API running at http://localhost:${PORT}/graphql`);
});