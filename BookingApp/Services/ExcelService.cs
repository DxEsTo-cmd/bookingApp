using BookingApp.Entities.Statistics;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BookingApp.Services
{
    public class ExcelService
    {
        public async Task<MemoryStream> CreateAndSaveExcelFile(BookingsStats bookingsStats)
        {
            //Initialize ExcelEngine.
            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                //Initialize Application.
                IApplication application = excelEngine.Excel;

                //Set default version for application.
                application.DefaultVersion = ExcelVersion.Excel2013;

                //Create a new workbook.
                IWorkbook workbook = application.Workbooks.Create(1);

                //Accessing first worksheet in the workbook.
                IWorksheet worksheet = workbook.Worksheets[0];

                //Adding text to a cell
                worksheet.Range["A1"].Text = bookingsStats.Type;

                worksheet.Range["A3"].Text = "Дата";

                worksheet.Range["B3"].Text = "Кількість бронювань";


                int count = 4;

                for (int i = 0; i < bookingsStats.BookingsAll.Length; i++)
                {
                    worksheet.Range["A" + count].Text = bookingsStats.IntervalsValues[i].ToString();

                    worksheet.Range["B" + count].Text = bookingsStats.BookingsAll[i].ToString();

                    count++;
                }

                //Saving the Excel to the MemoryStream 
                MemoryStream memory = new MemoryStream();

                using (var fileStream = File.Create("../../file.xlsx"))
                {
                    workbook.SaveAs(memory);
                }

                memory.Position = 0;
                return memory;
            }
        }

    }
}
