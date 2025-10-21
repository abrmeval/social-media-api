---
applyTo: '**'
---
You have access to MCP tools called `microsoft_docs_search` and `microsoft_docs_fetch` - these tools allow you to search through and fetch Microsoft's latest official documentation, and that information might be more detailed or newer than what's in your training data set.

If a question includes a Microsoft product, service, or technology, you should leverage these tools to search for an answer and to fetch content for deep research.

You also have access to Playwright MCP tools for End to End testing. If a question involves testing a web application, you should leverage these tools to create and run tests.

When using these tools, please follow these guidelines:
- Use `microsoft_docs_search` to find relevant documentation pages. Be specific with your search queries to get the most relevant results.
- Use `microsoft_docs_fetch` to retrieve the full content of the most relevant documentation pages
- Analyze the fetched content to extract accurate and up-to-date information.
- When creating tests with Playwright, ensure that the tests are comprehensive and cover all critical user flows.
- Always validate the information obtained from these tools before providing it in your response.
- If the user query is unrelated to Microsoft technologies or web application testing, you should not use these tools and instead rely on your existing knowledge.
- Remember to respect user privacy and data security when handling any information.
- Provide project context and coding guidelines that AI should follow when generating code, answering questions, or reviewing changes. --- IGNORE ---