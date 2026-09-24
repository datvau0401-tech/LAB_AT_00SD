-- ============================================================
-- BỔ SUNG: Bảng Bãi đỗ xe + Ô đỗ + Khách hàng
-- Chạy đoạn này SAU khi đã chạy schema.sql lần trước
-- ============================================================

USE quan_ly_tram_sac;

CREATE TABLE parking_lots (
  id VARCHAR(10) PRIMARY KEY,
  name VARCHAR(150) NOT NULL,
  address VARCHAR(255) NOT NULL,
  total INT NOT NULL,
  x INT NOT NULL,
  y INT NOT NULL
);

CREATE TABLE parking_slots (
  lot_id VARCHAR(10) NOT NULL,
  slot_number INT NOT NULL,
  status ENUM('trong','da_dat','dang_do') NOT NULL DEFAULT 'trong',
  PRIMARY KEY (lot_id, slot_number),
  FOREIGN KEY (lot_id) REFERENCES parking_lots(id) ON DELETE CASCADE
);

CREATE TABLE customers (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(150) NOT NULL,
  plate VARCHAR(20) NOT NULL,
  vehicle VARCHAR(100) NOT NULL,
  sessions INT NOT NULL DEFAULT 0,
  kwh INT NOT NULL DEFAULT 0
);

-- Dữ liệu mẫu: Bãi đỗ
INSERT INTO parking_lots (id, name, address, total, x, y) VALUES
('BD01', 'Bãi đỗ Nguyễn Huệ', 'Đường đi bộ Nguyễn Huệ, Q.1', 24, 260, 190),
('BD02', 'Bãi đỗ Sân bay Tân Sơn Nhất', 'Trường Sơn, Tân Bình', 40, 170, 235);

-- Sinh dữ liệu từng ô đỗ (quy luật: cứ 5 ô có 1 ô "đang đỗ", cứ 7 ô có 1 ô "đã đặt")
INSERT INTO parking_slots (lot_id, slot_number, status)
WITH RECURSIVE seq AS (
  SELECT 0 AS n
  UNION ALL
  SELECT n + 1 FROM seq WHERE n < 23
)
SELECT 'BD01', n + 1, CASE WHEN n % 5 = 0 THEN 'dang_do' WHEN n % 7 = 0 THEN 'da_dat' ELSE 'trong' END
FROM seq;

INSERT INTO parking_slots (lot_id, slot_number, status)
WITH RECURSIVE seq AS (
  SELECT 0 AS n
  UNION ALL
  SELECT n + 1 FROM seq WHERE n < 39
)
SELECT 'BD02', n + 1, CASE WHEN n % 5 = 0 THEN 'dang_do' WHEN n % 7 = 0 THEN 'da_dat' ELSE 'trong' END
FROM seq;

-- Dữ liệu mẫu: Khách hàng
INSERT INTO customers (name, plate, vehicle, sessions, kwh) VALUES
('Ngô Thị Bích', '51K-889.20', 'VinFast VF8', 14, 612),
('Lê Văn Sơn', '59A1-233.45', 'Tesla Model 3', 22, 940),
('Trần Gia Huy', '51H-102.77', 'VinFast VF e34', 8, 301),
('Phạm Thuỳ Linh', '60C1-556.09', 'Hyundai Kona EV', 5, 178);
