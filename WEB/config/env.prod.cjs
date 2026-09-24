// Placeholders: set the production API and SPA origins per project before building with QENV=prod.
module.exports = {
  API_BASE_URL: "https://api.example.com",
  WEB_BASE_URL: "https://app.example.com",
  BUILD_PUBLIC_PATH: "",
  PUBLISH_FOLDER: "",
  // Absolute output folder for `npm run build_prod` (see package-scripts.js) — overrides PUBLISH_FOLDER.
  PUBLISH_PATH: "D:/Vaibhav/Publish/Sakolee/Live",
  IGNORE_PUBLIC_FOLDER: false,
};
