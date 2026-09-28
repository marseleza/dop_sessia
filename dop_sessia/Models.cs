using System;
using System.Collections.Generic;
// using Npgsql;   // ← раскомментировать, когда появится пакет Npgsql

namespace FunctionPlotter
{
    // -------- Модель записи --------
    public class PlotRecord
    {
        public int Id { get; set; }
        public string FunctionName { get; set; }
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }
        public double XMin { get; set; }
        public double XMax { get; set; }
        public DateTime StartTime { get; set; }

        public override string ToString()
            => $"{FunctionName}(A={A}, B={B}, C={C}) [{XMin};{XMax}] @ {StartTime:HH:mm:ss}";
    }

    // -------- Репозиторий PostgreSQL (пока отключён) --------
    public static class PlotRepository
    {
        // ⚠️ Поменяйте под свои данные, когда включите БД
        // private const string ConnString =
        //     "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=function_plotter";

        // Локальный список вместо БД — чтобы приложение работало без Npgsql
        private static readonly List<PlotRecord> _memory = new List<PlotRecord>();
        private static int _nextId = 1;

        public static void EnsureTable()
        {
            // === ВАРИАНТ С POSTGRES (раскомментировать, когда будет Npgsql) ===
            // using (var conn = new NpgsqlConnection(ConnString))
            // {
            //     conn.Open();
            //     const string sql = @"
            //         CREATE TABLE IF NOT EXISTS plot_records (
            //             id            SERIAL PRIMARY KEY,
            //             function_name VARCHAR(10)      NOT NULL,
            //             a             DOUBLE PRECISION NOT NULL,
            //             b             DOUBLE PRECISION NOT NULL,
            //             c             DOUBLE PRECISION NOT NULL,
            //             x_min         DOUBLE PRECISION NOT NULL,
            //             x_max         DOUBLE PRECISION NOT NULL,
            //             start_time    TIMESTAMP        NOT NULL
            //         );";
            //     using (var cmd = new NpgsqlCommand(sql, conn))
            //         cmd.ExecuteNonQuery();
            // }

            // Заглушка: ничего не делает
        }

        public static void Add(PlotRecord r)
        {
            // === ВАРИАНТ С POSTGRES (раскомментировать, когда будет Npgsql) ===
            // using (var conn = new NpgsqlConnection(ConnString))
            // {
            //     conn.Open();
            //     const string sql = @"
            //         INSERT INTO plot_records
            //             (function_name, a, b, c, x_min, x_max, start_time)
            //         VALUES (@fn, @a, @b, @c, @xmin, @xmax, @st)";
            //     using (var cmd = new NpgsqlCommand(sql, conn))
            //     {
            //         cmd.Parameters.AddWithValue("fn",   r.FunctionName);
            //         cmd.Parameters.AddWithValue("a",    r.A);
            //         cmd.Parameters.AddWithValue("b",    r.B);
            //         cmd.Parameters.AddWithValue("c",    r.C);
            //         cmd.Parameters.AddWithValue("xmin", r.XMin);
            //         cmd.Parameters.AddWithValue("xmax", r.XMax);
            //         cmd.Parameters.AddWithValue("st",   r.StartTime);
            //         cmd.ExecuteNonQuery();
            //     }
            // }

            // Заглушка: пишем в память
            r.Id = _nextId++;
            _memory.Add(r);
        }

        public static List<PlotRecord> LoadLast(int count = 50)
        {
            // === ВАРИАНТ С POSTGRES (раскомментировать, когда будет Npgsql) ===
            // var list = new List<PlotRecord>();
            // using (var conn = new NpgsqlConnection(ConnString))
            // {
            //     conn.Open();
            //     string sql = @"
            //         SELECT id, function_name, a, b, c, x_min, x_max, start_time
            //         FROM plot_records
            //         ORDER BY start_time DESC
            //         LIMIT " + count;
            //     using (var cmd = new NpgsqlCommand(sql, conn))
            //     using (var rd = cmd.ExecuteReader())
            //     {
            //         while (rd.Read())
            //         {
            //             list.Add(new PlotRecord
            //             {
            //                 Id           = rd.GetInt32(0),
            //                 FunctionName = rd.GetString(1),
            //                 A            = rd.GetDouble(2),
            //                 B            = rd.GetDouble(3),
            //                 C            = rd.GetDouble(4),
            //                 XMin         = rd.GetDouble(5),
            //                 XMax         = rd.GetDouble(6),
            //                 StartTime    = rd.GetDateTime(7)
            //             });
            //         }
            //     }
            // }
            // return list;

            // Заглушка: отдаём из памяти
            var result = new List<PlotRecord>();
            int start = _memory.Count - count;
            if (start < 0) start = 0;
            for (int i = _memory.Count - 1; i >= start; i--)
                result.Add(_memory[i]);
            return result;
        }
    }
}