// Webpack require:
var scripts = require('./src/template/template.html');

module.exports = function () {
  let html = '<!DOCTYPE html><html><head>' +
    scripts +
    '</body> </html>';
  return html;
};
