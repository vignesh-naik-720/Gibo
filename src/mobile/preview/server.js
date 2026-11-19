const http = require('http');
const fs = require('fs');
const path = require('path');

const PORT = 3000;
const FILE = path.join(__dirname, 'index.html');

const server = http.createServer((req, res) => {
  fs.readFile(FILE, (err, data) => {
    if (err) {
      res.writeHead(500);
      return res.end('Error loading preview');
    }
    res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
    res.end(data);
  });
});

server.listen(PORT, '0.0.0.0', () => {
  console.log(`Gibo Mobile Preview Server active:`);
  console.log(`- Local laptop browser: http://localhost:${PORT}`);
  console.log(`- Phone browser (same Wi-Fi): http://192.168.1.10:${PORT}`);
});
