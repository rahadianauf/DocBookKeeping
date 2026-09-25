BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "biaya_operasional" (
	"id_biaya"	TEXT,
	"kategori"	TEXT NOT NULL,
	"nominal"	REAL NOT NULL,
	"tanggal"	TEXT NOT NULL DEFAULT (DATE('now')),
	"keterangan"	TEXT,
	PRIMARY KEY("id_biaya"),
	CHECK("nominal" >= 0)
);
CREATE TABLE IF NOT EXISTS "biaya_tambahan_batch" (
	"id"	INTEGER,
	"id_trans_masuk"	TEXT NOT NULL,
	"komponen"	TEXT NOT NULL,
	"nilai"	REAL NOT NULL,
	"keterangan"	TEXT,
	PRIMARY KEY("id" AUTOINCREMENT),
	FOREIGN KEY("id_trans_masuk") REFERENCES "trans_barang"("id_trans"),
	CHECK("nilai" >= 0)
);
CREATE TABLE IF NOT EXISTS "mst_barang" (
	"id_barang"	TEXT,
	"nama_barang"	TEXT NOT NULL,
	"id_kategori"	INTEGER NOT NULL,
	"id_satuan"	INTEGER NOT NULL,
	PRIMARY KEY("id_barang"),
	FOREIGN KEY("id_kategori") REFERENCES "mst_kategori"("id"),
	FOREIGN KEY("id_satuan") REFERENCES "mst_satuan"("id")
);
CREATE TABLE IF NOT EXISTS "mst_catatAll" (
	"id_trans"	TEXT NOT NULL,
	"periode"	TEXT,
	"tag"	TEXT,
	"nilai_masuk"	REAL,
	"nilai_keluar"	REAL,
	"keterangan"	INTEGER
);
CREATE TABLE IF NOT EXISTS "mst_jasa" (
	"id_jasa"	TEXT NOT NULL,
	"nama_jasa"	TEXT NOT NULL,
	"id_kategori"	INTEGER NOT NULL,
	PRIMARY KEY("id_jasa"),
	FOREIGN KEY("id_kategori") REFERENCES "mst_kategori"("id")
);
CREATE TABLE IF NOT EXISTS "mst_kategori" (
	"id"	INTEGER NOT NULL,
	"kategori"	TEXT NOT NULL,
	PRIMARY KEY("id" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "mst_pasien" (
	"id_pasien"	TEXT,
	"nama_pasien"	TEXT NOT NULL,
	"no_telepon"	TEXT,
	"alamat"	TEXT,
	"tanggal_daftar"	TEXT DEFAULT (DATE('now')),
	PRIMARY KEY("id_pasien")
);
CREATE TABLE IF NOT EXISTS "mst_pemasok" (
	"id_pemasok"	INTEGER,
	"nama_pemasok"	TEXT NOT NULL,
	"kontak"	TEXT,
	"alamat"	TEXT,
	PRIMARY KEY("id_pemasok" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "mst_resep" (
	"id_resep"	INTEGER,
	"id_barang_jadi"	TEXT NOT NULL,
	"id_barang_bahan"	TEXT NOT NULL,
	"jumlah_per_unit"	NUMERIC NOT NULL,
	PRIMARY KEY("id_resep" AUTOINCREMENT),
	FOREIGN KEY("id_barang_bahan") REFERENCES "mst_barang"("id_barang"),
	FOREIGN KEY("id_barang_jadi") REFERENCES "mst_barang"("id_barang"),
	CHECK("jumlah_per_unit" > 0),
	CHECK("id_barang_jadi" <> "id_barang_bahan")
);
CREATE TABLE IF NOT EXISTS "mst_satuan" (
	"id"	INTEGER NOT NULL,
	"kode"	TEXT NOT NULL,
	"satuan"	TEXT NOT NULL,
	"keterangan"	TEXT,
	PRIMARY KEY("id" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "mst_user" (
	"id"	INTEGER NOT NULL,
	"username"	TEXT NOT NULL,
	"password"	TEXT NOT NULL,
	PRIMARY KEY("id" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "pemakaian_batch" (
	"id"	INTEGER,
	"id_trans_keluar"	TEXT NOT NULL,
	"id_batch"	INTEGER NOT NULL,
	"jumlah_diambil"	INTEGER NOT NULL,
	"harga_pokok_satuan"	NUMERIC NOT NULL,
	"subtotal_nilai"	REAL NOT NULL,
	PRIMARY KEY("id" AUTOINCREMENT),
	FOREIGN KEY("id_batch") REFERENCES "stok_batch"("id_batch"),
	FOREIGN KEY("id_trans_keluar") REFERENCES "trans_barang"("id_trans"),
	CHECK("jumlah_diambil" > 0),
	CHECK("harga_pokok_satuan" >= 0)
);
CREATE TABLE IF NOT EXISTS "stok_batch" (
	"id_batch"	INTEGER,
	"id_barang"	TEXT NOT NULL,
	"id_trans_masuk"	TEXT NOT NULL,
	"tanggal_masuk"	TEXT NOT NULL,
	"jumlah_masuk"	INTEGER NOT NULL,
	"sisa_jumlah"	INTEGER NOT NULL,
	"harga_beli"	NUMERIC NOT NULL,
	"tanggal_kadaluwarsa"	TEXT,
	PRIMARY KEY("id_batch" AUTOINCREMENT),
	FOREIGN KEY("id_barang") REFERENCES "mst_barang"("id_barang"),
	FOREIGN KEY("id_trans_masuk") REFERENCES "trans_barang"("id_trans"),
	CHECK("jumlah_masuk" > 0),
	CHECK("sisa_jumlah" >= 0 AND "sisa_jumlah" <= "jumlah_masuk"),
	CHECK("harga_beli" >= 0)
);
CREATE TABLE IF NOT EXISTS "trans_barang" (
	"id_trans"	TEXT NOT NULL,
	"id_barang"	TEXT NOT NULL,
	"id_pemasok"	INTEGER,
	"tanggal_input"	TEXT NOT NULL DEFAULT (DATE('now')),
	"TAG"	TEXT,
	"sumber"	TEXT,
	"tujuan_keluar"	TEXT,
	"id_trans_produksi"	TEXT,
	"jumlah"	INTEGER NOT NULL,
	"harga_satuan"	NUMERIC NOT NULL,
	"nilai"	REAL NOT NULL,
	"tanggal_kadaluwarsa"	TEXT,
	"keterangan"	TEXT,
	PRIMARY KEY("id_trans"),
	FOREIGN KEY("id_barang") REFERENCES "mst_barang"("id_barang"),
	FOREIGN KEY("id_pemasok") REFERENCES "mst_pemasok"("id_pemasok"),
	FOREIGN KEY("id_trans_produksi") REFERENCES "trans_barang"("id_trans"),
	CHECK("sumber" IS NULL OR "sumber" IN ('BELI', 'PRODUKSI')),
	CHECK("tujuan_keluar" IS NULL OR "tujuan_keluar" IN ('JUAL', 'PRODUKSI', 'PAKAI_SENDIRI', 'RUSAK_HILANG')),
	CHECK("jumlah" > 0),
	CHECK("harga_satuan" >= 0),
	CHECK("tanggal_kadaluwarsa" IS NULL OR ("tanggal_kadaluwarsa" > "tanggal_input" AND "tanggal_kadaluwarsa" = date("tanggal_kadaluwarsa")))
);
CREATE TABLE IF NOT EXISTS "trans_jasa" (
	"id_trans"	TEXT NOT NULL,
	"tanggal_input"	TEXT NOT NULL DEFAULT (DATE('now')),
	"id_jasa"	TEXT NOT NULL,
	"id_pasien"	TEXT,
	"TAG"	TEXT,
	"harga"	REAL NOT NULL,
	"keterangan"	TEXT,
	PRIMARY KEY("id_trans"),
	FOREIGN KEY("id_jasa") REFERENCES "mst_jasa"("id_jasa"),
	FOREIGN KEY("id_pasien") REFERENCES "mst_pasien"("id_pasien")
);
CREATE UNIQUE INDEX IF NOT EXISTS "IX_mst_barang_nama_barang" ON "mst_barang" (
	"nama_barang"
);
CREATE TRIGGER trg_buat_stok_batch_saat_masuk
AFTER INSERT ON trans_barang
WHEN NEW.TAG = 'MASUK'
BEGIN
    INSERT INTO stok_batch (id_barang, id_trans_masuk, tanggal_masuk, jumlah_masuk, sisa_jumlah, harga_beli, tanggal_kadaluwarsa)
    VALUES (NEW.id_barang, NEW.id_trans, NEW.tanggal_input, NEW.jumlah, NEW.jumlah, NEW.harga_satuan, NEW.tanggal_kadaluwarsa);
END;
CREATE TRIGGER trg_cegah_hapus_batch_terpakai
BEFORE DELETE ON stok_batch
WHEN OLD.jumlah_masuk != OLD.sisa_jumlah
BEGIN
    SELECT RAISE(ABORT, 'Batch sudah pernah dipakai (FIFO), tidak boleh dihapus');
END;
CREATE TRIGGER trg_fifo_keluar_ambil_batch
AFTER INSERT ON trans_barang
WHEN NEW.TAG = 'KELUAR'
BEGIN
    INSERT INTO pemakaian_batch (id_trans_keluar, id_batch, jumlah_diambil, harga_pokok_satuan, subtotal_nilai)
    SELECT
        NEW.id_trans,
        id_batch,
        take_qty,
        harga_beli,
        ROUND(take_qty * harga_beli, 2)
    FROM (
        SELECT
            id_batch,
            harga_beli,
            sisa_jumlah,
            MAX(0, MIN(sisa_jumlah, NEW.jumlah - running_before)) AS take_qty
        FROM (
            SELECT
                id_batch,
                harga_beli,
                sisa_jumlah,
                COALESCE(SUM(sisa_jumlah) OVER (
                    ORDER BY tanggal_masuk, id_batch
                    ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING
                ), 0) AS running_before
            FROM stok_batch
            WHERE id_barang = NEW.id_barang AND sisa_jumlah > 0
        )
    )
    WHERE take_qty > 0;

    UPDATE stok_batch
    SET sisa_jumlah = sisa_jumlah - (
        SELECT COALESCE(SUM(jumlah_diambil), 0)
        FROM pemakaian_batch
        WHERE pemakaian_batch.id_batch = stok_batch.id_batch
          AND pemakaian_batch.id_trans_keluar = NEW.id_trans
    )
    WHERE id_batch IN (
        SELECT id_batch FROM pemakaian_batch WHERE id_trans_keluar = NEW.id_trans
    );
END;
CREATE TRIGGER trg_fifo_keluar_cek_stok
BEFORE INSERT ON trans_barang
WHEN NEW.TAG = 'KELUAR'
BEGIN
    SELECT RAISE(ABORT, 'Stok tidak mencukupi untuk barang ini')
    WHERE (
        SELECT COALESCE(SUM(sisa_jumlah), 0)
        FROM stok_batch
        WHERE id_barang = NEW.id_barang
    ) < NEW.jumlah;
END;
CREATE TRIGGER trg_hapus_pemakaian_saat_keluar_dihapus
BEFORE DELETE ON trans_barang
WHEN OLD.TAG = 'KELUAR'
BEGIN
    DELETE FROM pemakaian_batch WHERE id_trans_keluar = OLD.id_trans;
END;
CREATE TRIGGER trg_hapus_stok_batch_saat_masuk_dihapus
BEFORE DELETE ON trans_barang
WHEN OLD.TAG = 'MASUK'
BEGIN
    DELETE FROM stok_batch WHERE id_trans_masuk = OLD.id_trans;
END;
CREATE TRIGGER trg_hitung_nilai_barang_insert
AFTER INSERT ON trans_barang
BEGIN
    UPDATE trans_barang
    SET nilai = NEW.jumlah * NEW.harga_satuan
    WHERE id_trans = NEW.id_trans;
END;
CREATE TRIGGER trg_hitung_nilai_barang_update
AFTER UPDATE OF jumlah, harga_satuan ON trans_barang
WHEN NEW.nilai != NEW.jumlah * NEW.harga_satuan
BEGIN
    UPDATE trans_barang
    SET nilai = NEW.jumlah * NEW.harga_satuan
    WHERE id_trans = NEW.id_trans;
END;
CREATE TRIGGER trg_kembalikan_sisa_saat_hapus_pemakaian
AFTER DELETE ON pemakaian_batch
BEGIN
    UPDATE stok_batch
    SET sisa_jumlah = sisa_jumlah + OLD.jumlah_diambil
    WHERE id_batch = OLD.id_batch;
END;
CREATE TRIGGER trg_larang_edit_keluar
BEFORE UPDATE OF id_barang, jumlah, harga_satuan, tujuan_keluar, keterangan ON trans_barang
WHEN OLD.TAG = 'KELUAR'
BEGIN
    SELECT RAISE(ABORT, 'Transaksi KELUAR tidak boleh diedit. Hapus transaksi ini lalu buat transaksi baru.');
END;
CREATE TRIGGER trg_sync_stok_batch_saat_masuk_diupdate
AFTER UPDATE OF jumlah, harga_satuan, id_barang, tanggal_input, tanggal_kadaluwarsa
ON trans_barang
WHEN OLD.TAG = 'MASUK' AND NEW.TAG = 'MASUK'
BEGIN
    SELECT RAISE(ABORT, 'Jumlah baru lebih kecil dari yang sudah terpakai FIFO, tidak bisa diubah')
    WHERE EXISTS (
        SELECT 1 FROM stok_batch
        WHERE id_trans_masuk = NEW.id_trans
          AND NEW.jumlah < (jumlah_masuk - sisa_jumlah)
    );

    UPDATE stok_batch
    SET id_barang     = NEW.id_barang,
        tanggal_masuk = NEW.tanggal_input,
        sisa_jumlah   = sisa_jumlah + (NEW.jumlah - jumlah_masuk),
        jumlah_masuk  = NEW.jumlah,
        tanggal_kadaluwarsa = NEW.tanggal_kadaluwarsa
    WHERE id_trans_masuk = NEW.id_trans;

    UPDATE stok_batch
    SET harga_beli = ROUND((
        (NEW.jumlah * NEW.harga_satuan)
        + (SELECT COALESCE(SUM(nilai), 0) FROM biaya_tambahan_batch WHERE id_trans_masuk = NEW.id_trans)
    ) * 1.0 / NEW.jumlah, 2)
    WHERE id_trans_masuk = NEW.id_trans;
END;
CREATE TRIGGER trg_update_harga_beli_biaya_delete
AFTER DELETE ON biaya_tambahan_batch
BEGIN
    UPDATE stok_batch
    SET harga_beli = ROUND((
        (SELECT jumlah * harga_satuan FROM trans_barang WHERE id_trans = OLD.id_trans_masuk)
        + (SELECT COALESCE(SUM(nilai), 0) FROM biaya_tambahan_batch WHERE id_trans_masuk = OLD.id_trans_masuk)
    ) * 1.0 / jumlah_masuk, 2)
    WHERE id_trans_masuk = OLD.id_trans_masuk;
END;
CREATE TRIGGER trg_update_harga_beli_biaya_insert
AFTER INSERT ON biaya_tambahan_batch
BEGIN
    UPDATE stok_batch
    SET harga_beli = ROUND((
        (SELECT jumlah * harga_satuan FROM trans_barang WHERE id_trans = NEW.id_trans_masuk)
        + (SELECT COALESCE(SUM(nilai), 0) FROM biaya_tambahan_batch WHERE id_trans_masuk = NEW.id_trans_masuk)
    ) * 1.0 / jumlah_masuk, 2)
    WHERE id_trans_masuk = NEW.id_trans_masuk;
END;
CREATE TRIGGER trg_update_harga_beli_biaya_update
AFTER UPDATE OF nilai ON biaya_tambahan_batch
BEGIN
    UPDATE stok_batch
    SET harga_beli = ROUND((
        (SELECT jumlah * harga_satuan FROM trans_barang WHERE id_trans = NEW.id_trans_masuk)
        + (SELECT COALESCE(SUM(nilai), 0) FROM biaya_tambahan_batch WHERE id_trans_masuk = NEW.id_trans_masuk)
    ) * 1.0 / jumlah_masuk, 2)
    WHERE id_trans_masuk = NEW.id_trans_masuk;
END;
COMMIT;
