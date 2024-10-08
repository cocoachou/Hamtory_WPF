﻿using CsvHelper;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace Hamtory_WPF
{
    public partial class RawData : Window
    {
        private readonly DateTime startDateTime;
        private readonly DateTime endDateTime;
        private DataTable filteredDataTable;

        public RawData(DateTime startDateTime, DateTime endDateTime)
        {
            InitializeComponent();
            this.startDateTime = startDateTime;
            this.endDateTime = endDateTime;
            LoadData(); // 창이 열릴 때 자동으로 데이터 로드
        }

        private void LoadData()
        {
            try
            {
                // CSV 파일이 존재하는지 확인
                var csvFilePath = @"melting_tank.csv";
                if (File.Exists(csvFilePath))
                {
                    // CSV 파일을 로드하고 DataGrid에 표시
                    DataTable dataTable = LoadCsvData(csvFilePath);
                    filteredDataTable = FilterDataByDateRange(dataTable, startDateTime, endDateTime);
                    DisplayData(filteredDataTable);
                }
                else
                {
                    MessageBox.Show("CSV 파일을 찾을 수 없습니다."); // 파일이 없는 경우 오류 메시지 표시
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터 로드 중 오류 발생: {ex.Message}"); // 오류 발생 시 메시지 표시
            }
        }

        private DataTable LoadCsvData(string filePath)
        {
            var dataTable = new DataTable();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<dynamic>().ToList();

                if (records.Count > 0)
                {
                    foreach (var header in ((IDictionary<string, object>)records[0]).Keys)
                    {
                        dataTable.Columns.Add(header);
                    }

                    foreach (var record in records)
                    {
                        var row = dataTable.NewRow();
                        foreach (var kvp in (IDictionary<string, object>)record)
                        {
                            row[kvp.Key] = kvp.Value;
                        }
                        dataTable.Rows.Add(row);
                    }
                }
            }

            return dataTable;
        }

        private DataTable FilterDataByDateRange(DataTable dataTable, DateTime startDateTime, DateTime endDateTime)
        {
            var filteredDataTable = dataTable.Clone();
            foreach (DataRow row in dataTable.Rows)
            {
                if (DateTime.TryParse(row["STD_DT"].ToString(), out DateTime rowDateTime))
                {
                    if (rowDateTime >= startDateTime && rowDateTime <= endDateTime)
                    {
                        filteredDataTable.ImportRow(row);
                    }
                }
            }
            return filteredDataTable;
        }

        private void DisplayData(DataTable dataTable)
        {
            dataGrid.ItemsSource = dataTable.DefaultView;
        }
        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (filteredDataTable != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "엑셀 파일 저장"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(filteredDataTable, "FilteredData");
                        wb.SaveAs(saveFileDialog.FileName);
                    }
                }
            }
            else
            {
                MessageBox.Show("필터링된 데이터가 없습니다.");
            }
        }
    }
}
