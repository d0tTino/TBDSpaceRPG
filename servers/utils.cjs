function logError(err) {
  console.error(err);
}

function parseJson(body) {
  if (!body || !String(body).trim()) return null;
  try {
    return JSON.parse(body);
  } catch (err) {
    logError(err);
    return null;
  }
}

module.exports = { logError, parseJson };
