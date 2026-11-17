// Gera os ícones do PWA em public/icons (PNG sem dependências: zlib + CRC na mão).
// Mesmo desenho do favicon: fundo escuro e três barras (cinza, cinza claro, verde).
// Rodar: node apps/web/scripts/generate-icons.mjs
import { writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { deflateSync } from 'node:zlib';

const outDir = join(dirname(fileURLToPath(import.meta.url)), '..', 'public', 'icons');
const BG = [23, 23, 23];
const BARS = [
  { x: 0.2, h: 0.28, color: [163, 163, 163] },
  { x: 0.42, h: 0.44, color: [212, 212, 212] },
  { x: 0.64, h: 0.6, color: [16, 185, 129] },
];

function icon(size, { rounded, safeZone }) {
  const px = Buffer.alloc(size * size * 4);
  const radius = rounded ? size * 0.22 : 0;
  // Ícone "maskable": o sistema corta as bordas, então o desenho encolhe para a zona segura (80%).
  const scale = safeZone ? 0.8 : 1;
  const offset = (size * (1 - scale)) / 2;
  const barW = 0.16 * size * scale;
  const base = offset + 0.8 * size * scale;
  for (let y = 0; y < size; y++) {
    for (let x = 0; x < size; x++) {
      const i = (y * size + x) * 4;
      let inside = true;
      if (radius) {
        const cx = Math.min(x, size - 1 - x);
        const cy = Math.min(y, size - 1 - y);
        if (cx < radius && cy < radius) {
          const dx = radius - cx - 0.5;
          const dy = radius - cy - 0.5;
          inside = dx * dx + dy * dy <= radius * radius;
        }
      }
      let color = BG;
      for (const bar of BARS) {
        const bx = offset + bar.x * size * scale;
        const top = base - bar.h * size * scale;
        if (x >= bx && x < bx + barW && y >= top && y < base) color = bar.color;
      }
      px[i] = color[0];
      px[i + 1] = color[1];
      px[i + 2] = color[2];
      px[i + 3] = inside ? 255 : 0;
    }
  }
  return png(size, px);
}

const crcTable = Array.from({ length: 256 }, (_, n) => {
  let c = n;
  for (let k = 0; k < 8; k++) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1;
  return c >>> 0;
});
const crc = (buf) => {
  let c = 0xffffffff;
  for (const v of buf) c = crcTable[(c ^ v) & 255] ^ (c >>> 8);
  return (c ^ 0xffffffff) >>> 0;
};
const chunk = (type, data) => {
  const len = Buffer.alloc(4);
  len.writeUInt32BE(data.length);
  const td = Buffer.concat([Buffer.from(type), data]);
  const c = Buffer.alloc(4);
  c.writeUInt32BE(crc(td));
  return Buffer.concat([len, td, c]);
};
function png(size, px) {
  const raw = Buffer.alloc((size * 4 + 1) * size);
  for (let y = 0; y < size; y++)
    px.copy(raw, y * (size * 4 + 1) + 1, y * size * 4, (y + 1) * size * 4);
  const ihdr = Buffer.alloc(13);
  ihdr.writeUInt32BE(size, 0);
  ihdr.writeUInt32BE(size, 4);
  ihdr[8] = 8;
  ihdr[9] = 6;
  return Buffer.concat([
    Buffer.from([137, 80, 78, 71, 13, 10, 26, 10]),
    chunk('IHDR', ihdr),
    chunk('IDAT', deflateSync(raw)),
    chunk('IEND', Buffer.alloc(0)),
  ]);
}

const files = {
  'icon-192.png': icon(192, { rounded: true, safeZone: false }),
  'icon-512.png': icon(512, { rounded: true, safeZone: false }),
  // Maskable e Apple: quadrado cheio, sem transparência (o sistema aplica a máscara).
  'maskable-512.png': icon(512, { rounded: false, safeZone: true }),
  'apple-touch-icon.png': icon(180, { rounded: false, safeZone: true }),
};
for (const [name, data] of Object.entries(files)) {
  writeFileSync(join(outDir, name), data);
  console.log(name, data.length, 'bytes');
}
