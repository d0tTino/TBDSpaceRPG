import js from "@eslint/js";
import globals from "globals";

export default [
  js.configs.recommended,
  {
    files: ["**/*.js", "**/*.cjs"],
    languageOptions: {
      ecmaVersion: 2020,
      sourceType: "script",
      globals: globals.node,
    },
    rules: {
    },
  },
];
