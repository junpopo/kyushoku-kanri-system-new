import fs from "node:fs/promises";
import path from "node:path";

const outputPath = process.argv[2];
if (!outputPath) {
  throw new Error("出力先CSVファイルを指定してください。");
}

let seed = 20260624;
function random() {
  seed = (seed * 1664525 + 1013904223) >>> 0;
  return seed / 0x100000000;
}

function randomInt(min, max) {
  return Math.floor(random() * (max - min + 1)) + min;
}

const lastNames = [
  "佐藤", "鈴木", "高橋", "田中", "伊藤",
  "渡辺", "山本", "中村", "小林", "加藤",
  "吉田", "山田", "佐々木", "山口", "松本",
  "井上", "木村", "林", "清水", "斎藤",
];
const firstNames = [
  "陽翔", "蓮", "湊", "結菜", "葵",
  "凛", "悠真", "芽依", "大和", "美咲",
  "颯太", "さくら", "樹", "莉子", "朝陽",
];
const altNames = [
  "Smith John", "Brown Emily", "Wilson James", "Taylor Emma", "Davis Michael",
];

const types = [
  ...Array(60).fill("生徒"),
  ...Array(15).fill("職員"),
  ...Array(5).fill("ALT"),
  ...Array(8).fill("教育実習生"),
  ...Array(6).fill("試食会"),
  ...Array(6).fill("ゲスト"),
];

for (let i = types.length - 1; i > 0; i--) {
  const swapIndex = randomInt(0, i);
  [types[i], types[swapIndex]] = [types[swapIndex], types[i]];
}

const typeCounters = new Map();
const classCounters = new Map();
const rows = [];

for (const type of types) {
  const typeNumber = (typeCounters.get(type) ?? 0) + 1;
  typeCounters.set(type, typeNumber);

  let grade = "";
  let className = "";
  let number = "";
  if (type === "生徒") {
    grade = String(randomInt(1, 6));
    className = String(randomInt(1, 3));
    const classKey = `${grade}-${className}`;
    number = String((classCounters.get(classKey) ?? 0) + 1);
    classCounters.set(classKey, Number(number));
  }

  let fullName;
  if (type === "ALT") {
    fullName = altNames[typeNumber - 1];
  } else {
    const nameIndex = rows.length;
    const lastName = lastNames[nameIndex % lastNames.length];
    const firstName = firstNames[Math.floor(nameIndex / lastNames.length) % firstNames.length];
    fullName = `${lastName} ${firstName}`;
  }

  rows.push([type, grade, className, number, fullName, "2026/4/1"]);
}

function csvValue(value) {
  const text = String(value);
  return `"${text.replaceAll('"', '""')}"`;
}

const csv = [
  ["区分", "学年", "組", "番号", "氏名", "開始日"],
  ...rows,
].map((row) => row.map(csvValue).join(",")).join("\r\n");

await fs.mkdir(path.dirname(path.resolve(outputPath)), { recursive: true });
await fs.writeFile(outputPath, `\uFEFF${csv}\r\n`, "utf8");
