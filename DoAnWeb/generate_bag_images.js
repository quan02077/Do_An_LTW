const fs = require('fs');
const path = require('path');
const zlib = require('zlib');

// CRC32 table
let crcTable = null;
function crc32(buf) {
  if (!crcTable) {
    crcTable = new Uint32Array(256);
    for (let i = 0; i < 256; i++) {
      let c = i;
      for (let k = 0; k < 8; k++) c = (c & 1) ? (0xedb88320 ^ (c >>> 1)) : (c >>> 1);
      crcTable[i] = c;
    }
  }
  let crc = 0xffffffff;
  for (let i = 0; i < buf.length; i++) crc = crcTable[(crc ^ buf[i]) & 0xff] ^ (crc >>> 8);
  return (crc ^ 0xffffffff) >>> 0;
}

function chunk(type, data) {
  const len = Buffer.alloc(4);
  len.writeUInt32BE(data.length, 0);
  const typeBuf = Buffer.from(type, 'ascii');
  const crcBuf = Buffer.alloc(4);
  const crc = crc32(Buffer.concat([typeBuf, data]));
  crcBuf.writeUInt32BE(crc, 0);
  return Buffer.concat([len, typeBuf, data, crcBuf]);
}

function makePNG(width, height, getPixel) {
  const bytesPerPixel = 4;
  const stride = width * bytesPerPixel;
  const raw = Buffer.alloc(height * (stride + 1));
  for (let y = 0; y < height; y++) {
    const rowOffset = y * (stride + 1);
    raw[rowOffset] = 0;
    for (let x = 0; x < width; x++) {
      const [r, g, b, a] = getPixel(x, y);
      const pxOffset = rowOffset + 1 + x * bytesPerPixel;
      raw[pxOffset] = r;
      raw[pxOffset + 1] = g;
      raw[pxOffset + 2] = b;
      raw[pxOffset + 3] = a;
    }
  }
  const compressed = zlib.deflateSync(raw);
  const sig = Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]);
  const ihdr = Buffer.alloc(13);
  ihdr.writeUInt32BE(width, 0);
  ihdr.writeUInt32BE(height, 4);
  ihdr[8] = 8;
  ihdr[9] = 6;
  ihdr[10] = 0;
  ihdr[11] = 0;
  ihdr[12] = 0;

  return Buffer.concat([
    sig,
    chunk('IHDR', ihdr),
    chunk('IDAT', compressed),
    chunk('IEND', Buffer.alloc(0))
  ]);
}

// 15 products info from SQL
const bags = [
  // CHARLES & KEITH
  { id: 1, name: 'Túi đeo chéo nữ CHARLES & KEITH', brand: 'CHARLES & KEITH', type: 'Túi đeo chéo', color1: [35, 35, 40], color2: [212, 175, 55], shape: 'crossbody' },
  { id: 2, name: 'Túi đeo vai nữ CHARLES & KEITH', brand: 'CHARLES & KEITH', type: 'Túi đeo vai', color1: [235, 225, 210], color2: [180, 140, 90], shape: 'shoulder' },
  { id: 3, name: 'Clutch dự tiệc nữ CHARLES & KEITH', brand: 'CHARLES & KEITH', type: 'Clutch', color1: [195, 160, 90], color2: [245, 230, 180], shape: 'clutch' },

  // NATOLI
  { id: 4, name: 'Túi đeo chéo nữ Natoli', brand: 'Natoli', type: 'Túi đeo chéo', color1: [220, 100, 80], color2: [255, 255, 255], shape: 'crossbody' },
  { id: 5, name: 'Túi tote nam nữ Natoli', brand: 'Natoli', type: 'Túi tote', color1: [230, 220, 200], color2: [80, 70, 60], shape: 'tote' },
  { id: 6, name: 'Balo nam Natoli', brand: 'Natoli', type: 'Balo', color1: [40, 55, 75], color2: [20, 25, 35], shape: 'backpack' },

  // PEDRO
  { id: 7, name: 'Túi đeo chéo nam nữ Pedro', brand: 'Pedro', type: 'Túi đeo chéo', color1: [45, 45, 50], color2: [160, 160, 170], shape: 'crossbody' },
  { id: 8, name: 'Túi đeo vai nam Pedro', brand: 'Pedro', type: 'Túi đeo vai', color1: [80, 50, 30], color2: [130, 85, 55], shape: 'shoulder' },
  { id: 9, name: 'Clutch dự tiệc nữ Pedro', brand: 'Pedro', type: 'Clutch', color1: [25, 25, 30], color2: [220, 180, 100], shape: 'clutch' },

  // ELLY
  { id: 10, name: 'Túi đeo chéo nữ ELLY', brand: 'ELLY', type: 'Túi đeo chéo', color1: [160, 35, 50], color2: [215, 170, 70], shape: 'crossbody' },
  { id: 11, name: 'Túi đeo vai nữ ELLY', brand: 'ELLY', type: 'Túi đeo vai', color1: [210, 195, 180], color2: [60, 45, 40], shape: 'shoulder' },
  { id: 12, name: 'Balo nam nữ ELLY', brand: 'ELLY', type: 'Balo', color1: [55, 40, 35], color2: [180, 140, 75], shape: 'backpack' },

  // JAMLOS
  { id: 13, name: 'Balo nam Jamlos', brand: 'Jamlos', type: 'Balo', color1: [65, 80, 65], color2: [40, 50, 40], shape: 'backpack' },
  { id: 14, name: 'Túi đeo chéo nam nữ Jamlos', brand: 'Jamlos', type: 'Túi đeo chéo', color1: [185, 115, 50], color2: [240, 235, 220], shape: 'crossbody' },
  { id: 15, name: 'Túi tote nam nữ Jamlos', brand: 'Jamlos', type: 'Túi tote', color1: [225, 215, 195], color2: [45, 60, 50], shape: 'tote' }
];

const W = 400;
const H = 400;
const outDir = path.join(__dirname, 'hinhAnh');
if (!fs.existsSync(outDir)) fs.mkdirSync(outDir, { recursive: true });

bags.forEach(bag => {
  for (let view = 1; view <= 3; view++) {
    const filename = `tui${bag.id}_${view}.png`;
    const filepath = path.join(outDir, filename);

    const pngBuf = makePNG(W, H, (x, y) => {
      // Background subtle gradient
      const bgGrad = Math.floor(245 + 8 * (y / H));
      let r = bgGrad, g = bgGrad, b = bgGrad + 2, a = 255;

      // Center coords
      const cx = W / 2 + (view === 2 ? 20 : (view === 3 ? -15 : 0));
      const cy = H / 2 + 20;

      // Draw shadow
      const dxS = (x - cx) / 130;
      const dyS = (y - (cy + 95)) / 22;
      if (dxS * dxS + dyS * dyS < 1) {
        const dist = Math.sqrt(dxS * dxS + dyS * dyS);
        const shadowAlpha = (1 - dist) * 0.25;
        r = Math.floor(r * (1 - shadowAlpha) + 60 * shadowAlpha);
        g = Math.floor(g * (1 - shadowAlpha) + 60 * shadowAlpha);
        b = Math.floor(b * (1 - shadowAlpha) + 70 * shadowAlpha);
      }

      // Drawing shapes based on bag type
      let inBag = false;
      let inStrap = false;
      let inAccent = false;
      let inFlap = false;
      let shade = 1.0;

      if (bag.shape === 'backpack') {
        // Backpack main body
        const bw = 90, bh = 110;
        const bTop = cy - 80, bBot = cy + 85;
        const bL = cx - bw, bR = cx + bw;
        if (x >= bL && x <= bR && y >= bTop && y <= bBot) {
          // rounded top
          if (y < bTop + 40) {
            const rx = (x - cx) / bw;
            const ry = (y - (bTop + 40)) / 40;
            if (rx * rx + ry * ry <= 1) inBag = true;
          } else {
            inBag = true;
          }
        }
        // Front pocket
        if (x >= cx - 65 && x <= cx + 65 && y >= cy + 10 && y <= cy + 75) {
          inAccent = true;
        }
        // Top handle
        const hdx = (x - cx) / 30;
        const hdy = (y - (bTop - 15)) / 20;
        if (hdx * hdx + hdy * hdy <= 1 && hdx * hdx + hdy * hdy >= 0.5 && y <= bTop) {
          inStrap = true;
        }
      } else if (bag.shape === 'tote') {
        // Tote bag trapezoid
        const topW = 100, botW = 85;
        const tTop = cy - 50, tBot = cy + 90;
        if (y >= tTop && y <= tBot) {
          const progress = (y - tTop) / (tBot - tTop);
          const curW = topW * (1 - progress) + botW * progress;
          if (Math.abs(x - cx) <= curW) inBag = true;
        }
        // Dual handles
        const h1 = Math.abs(x - (cx - 40));
        const h2 = Math.abs(x - (cx + 40));
        if ((h1 <= 7 || h2 <= 7) && y >= cy - 110 && y <= tTop) {
          inStrap = true;
        }
        if (Math.abs(x - cx) <= 40 && y >= cy - 115 && y <= cy - 103) {
          inStrap = true;
        }
      } else if (bag.shape === 'clutch') {
        // Sleek horizontal rectangle
        const cw = 115, ch = 65;
        if (Math.abs(x - cx) <= cw && Math.abs(y - cy) <= ch) {
          inBag = true;
          // envelope flap
          const flapSlope = Math.abs(x - cx) / cw;
          if (y <= cy - 10 + flapSlope * 40) inFlap = true;
        }
        // metallic lock
        if (Math.abs(x - cx) <= 15 && Math.abs(y - (cy + 15)) <= 10) inAccent = true;
      } else if (bag.shape === 'shoulder') {
        // Elegant curved shoulder bag
        const sw = 105, sh = 75;
        const dX = (x - cx) / sw;
        const dY = (y - (cy + 15)) / sh;
        if (dX * dX + dY * dY <= 1 && y >= cy - 45) inBag = true;
        // top curve
        if (Math.abs(x - cx) <= sw && y >= cy - 45 && y <= cy + 60) inBag = true;
        // Shoulder strap arc
        const strapR = Math.sqrt(Math.pow((x - cx) / 85, 2) + Math.pow((y - (cy - 40)) / 75, 2));
        if (strapR >= 0.92 && strapR <= 1.05 && y < cy) inStrap = true;
        // Lock
        if (Math.abs(x - cx) <= 12 && Math.abs(y - (cy + 10)) <= 12) inAccent = true;
      } else {
        // Crossbody bag
        const rw = 95, rh = 70;
        if (Math.abs(x - cx) <= rw && y >= cy - 35 && y <= cy + 70) inBag = true;
        // Flap
        if (Math.abs(x - cx) <= rw + 2 && y >= cy - 40 && y <= cy + 25) inFlap = true;
        // Crossbody chain / strap
        const strX = (x - (cx - 85)) + (y - (cy - 110)) * 0.4;
        if (Math.abs(strX) <= 5 && y <= cy - 35 && y >= cy - 120) inStrap = true;
        const strX2 = (x - (cx + 85)) - (y - (cy - 110)) * 0.4;
        if (Math.abs(strX2) <= 5 && y <= cy - 35 && y >= cy - 120) inStrap = true;
        // Clasp
        if (Math.abs(x - cx) <= 14 && Math.abs(y - (cy + 25)) <= 12) inAccent = true;
      }

      // Shading calculation
      const lightX = cx - 70, lightY = cy - 80;
      const ldist = Math.sqrt(Math.pow(x - lightX, 2) + Math.pow(y - lightY, 2));
      shade = Math.max(0.65, Math.min(1.25, 1.2 - ldist / 320));

      if (view === 2) shade *= (x > cx ? 0.85 : 1.1); // side light angle
      if (view === 3) shade *= (y > cy ? 0.9 : 1.15); // top angle

      if (inAccent) {
        r = Math.min(255, Math.floor(bag.color2[0] * shade * 1.1));
        g = Math.min(255, Math.floor(bag.color2[1] * shade * 1.1));
        b = Math.min(255, Math.floor(bag.color2[2] * shade * 1.1));
      } else if (inFlap) {
        r = Math.min(255, Math.floor(bag.color1[0] * shade * 1.08));
        g = Math.min(255, Math.floor(bag.color1[1] * shade * 1.08));
        b = Math.min(255, Math.floor(bag.color1[2] * shade * 1.08));
      } else if (inBag) {
        r = Math.min(255, Math.floor(bag.color1[0] * shade));
        g = Math.min(255, Math.floor(bag.color1[1] * shade));
        b = Math.min(255, Math.floor(bag.color1[2] * shade));
      } else if (inStrap) {
        const sc = bag.color2[0] > 180 ? bag.color2 : bag.color1;
        r = Math.min(255, Math.floor(sc[0] * shade));
        g = Math.min(255, Math.floor(sc[1] * shade));
        b = Math.min(255, Math.floor(sc[2] * shade));
      }

      return [r, g, b, a];
    });

    fs.writeFileSync(filepath, pngBuf);
  }
});

// Brand logos / images
const brands = [
  { name: 'ck.jpg', label: 'CHARLES & KEITH', bg: [20, 20, 25], fg: [255, 255, 255] },
  { name: 'natoli.jpg', label: 'NATOLI', bg: [220, 50, 45], fg: [255, 255, 255] },
  { name: 'pedro.jpg', label: 'PEDRO', bg: [30, 30, 30], fg: [215, 180, 100] },
  { name: 'elly.jpg', label: 'ELLY', bg: [110, 20, 35], fg: [255, 240, 210] },
  { name: 'jamlos.jpg', label: 'JAMLOS', bg: [50, 75, 55], fg: [245, 240, 225] }
];

brands.forEach(b => {
  const bPath = path.join(outDir, b.name);
  const buf = makePNG(350, 220, (x, y) => {
    // Card with rounded border
    const borderW = 4;
    const isBorder = (x < borderW || x >= 350 - borderW || y < borderW || y >= 220 - borderW);
    if (isBorder) return [200, 200, 200, 255];
    const grad = 1 - (y / 220) * 0.25;
    return [
      Math.floor(b.bg[0] * grad),
      Math.floor(b.bg[1] * grad),
      Math.floor(b.bg[2] * grad),
      255
    ];
  });
  fs.writeFileSync(bPath, buf);
});

// Banners for hero carousel
const bannerConfigs = [
  { name: 'banner1.jpg', bg1: [30, 35, 45], bg2: [15, 18, 24] },
  { name: 'banner2.jpg', bg1: [90, 30, 40], bg2: [40, 15, 20] },
  { name: 'banner3.jpg', bg1: [40, 60, 50], bg2: [20, 30, 25] }
];

bannerConfigs.forEach(bc => {
  const bPath = path.join(outDir, bc.name);
  const buf = makePNG(1200, 480, (x, y) => {
    const t = (x + y * 0.5) / (1200 + 240);
    const r = Math.floor(bc.bg1[0] * (1 - t) + bc.bg2[0] * t);
    const g = Math.floor(bc.bg1[1] * (1 - t) + bc.bg2[1] * t);
    const b = Math.floor(bc.bg1[2] * (1 - t) + bc.bg2[2] * t);
    return [r, g, b, 255];
  });
  fs.writeFileSync(bPath, buf);
});

console.log('Successfully generated 45 bag images, 5 brand cards, and 3 banners in DoAnWeb/hinhAnh!');

