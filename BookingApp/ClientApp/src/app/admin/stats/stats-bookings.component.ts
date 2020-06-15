import { Component, OnInit, ViewChild } from '@angular/core';
import { StatsService } from '../../services/stats.service';
import { BookingStats } from '../../models/stats-booking';
import { Logger } from '../../services/logger.service';
import { ChartOptions, ChartType, ChartDataSets } from 'chart.js';
import { saveAs } from 'file-saver';
import { HttpEventType } from '@angular/common/http';

@Component({
  selector: 'app-stats-booking',
  templateUrl: './stats-bookings.component.html',
  styleUrls: ['./stats-bookings.component.css']
})
export class StatsBookingComponent implements OnInit {

  bookingStats: BookingStats;
  bookingsWithKeys: BookingsWithKeys[] = [];
  defaultFromDate: Date;
  defaultToDate: Date;

  fromDateString: string;
  toDateString: string;

  intervalSelect: string = 'day';

  actionsTypeSelect: string = 'creations';
  
  public SystemName: string = "MF1";
  firstCopy = false;

  // data
  public lineChartData: Array<number> = [];
 
  public labelMFL: Array<any> = [
      { data: this.lineChartData,
        label: this.SystemName
      }
  ];
  // labels
  public lineChartLabels: Array<any> = [];



  public barChartOptions: ChartOptions = {
    responsive: true,
    // We use these empty structures as placeholders for dynamic theming.
    scales: { xAxes: [{}], yAxes: [{}] },
    plugins: {
      datalabels: {
        anchor: 'end',
        align: 'end',
      }
    }
  };
  public lineChartOptions: any = {
    responsive: true,
    scales : {
      yAxes: [{
        ticks: {
          max : 60,
          min : 0,
        }
      }],
      xAxes: [{
  
 
        }],
    },
      plugins: {
      datalabels: {
        display: true,
        align: 'top',
        anchor: 'end',
        //color: "#2756B3",
        color: "#222",

        font: {
          family: 'FontAwesome',
          size: 14
        },
      
      },
      deferred: false

    },

  };

   _lineChartColors:Array<any> = [{
       backgroundColor: 'red',
        borderColor: 'red',
        pointBackgroundColor: 'red',
        pointBorderColor: 'red',
        pointHoverBackgroundColor: 'red',
        pointHoverBorderColor: 'red' 
      }];



  public ChartType = 'bar';
  public BaseChart:any;

  constructor(private statsService: StatsService ) {     
  }

  ngOnInit() {
    let currentDate: Date;
    currentDate = new Date();
    Logger.log('currentDate: '+currentDate);
    this.defaultToDate = new Date(currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate());
    this.defaultFromDate = new Date(this.defaultToDate.getTime() - (1000 * 60 * 60 * 24 * 7));

    this.fromDateString = this.dateToString(this.defaultFromDate);
    Logger.log('fromDate: ' + this.fromDateString);
    Logger.log('html fromDate: ' + (document.getElementById('fromDate') as HTMLInputElement).value);
    this.toDateString = this.dateToString(this.defaultToDate);
    Logger.log('toDate: ' + this.toDateString);
  }

  loadStats() {

    let start = this.stringToDate(this.fromDateString);
    let end = this.stringToDate(this.toDateString);

    Logger.log(start.toString());

    this.statsService.getBookingStats(this.actionsTypeSelect, start, end, this.intervalSelect).subscribe((res: BookingStats) => {
      this.bookingStats = res;
      this.bookingsWithKeys = [];
      this.lineChartData = [];
      this.lineChartLabels.length = 0;
      for (var i = 0; i < this.bookingStats.intervalsValues.length; i++) {
        let key: string;
        let date: Date = new Date(this.bookingStats.intervalsValues[i]);
        if (this.intervalSelect === 'day') {
          key = date.toDateString();
        }
        else if (this.intervalSelect === 'hour') {
          key = date.toString();
        }
        else if (this.intervalSelect === 'month') {
          key = date.getMonth().toString() +' '+  date.getFullYear().toString();
        }
        let keyValuePair: BookingsWithKeys = { key: key, value: this.bookingStats.bookingsAll[i] };       
        this.bookingsWithKeys.push(keyValuePair);    
        this.lineChartLabels.push(key);
        this.lineChartData.push(this.bookingStats.bookingsAll[i]);
      }
      
      this.labelMFL = [
        { data: this.lineChartData,
          label: this.SystemName
        }
      ];  
    });

    
  }

  dateToString(date: Date) {
    let year = date.getFullYear().toString();
    let month = date.getMonth().toString().length < 2 ? '0' + (date.getMonth()+1).toString() : (date.getMonth()+1).toString();
    let day = date.getDate().toString().length < 2 ? '0' + date.getDate().toString() : date.getDate().toString();
    return year + '-' + month + '-' + day;
  }

  stringToDate(s: string) {
    let year = Number(s.substr(0, 4));
    let month = Number(s.substr(5, 2)) - 1;
    let day = Number(s.substr(8, 2));    

    return new Date(year, month, day);
  }

  downloadFile() {
    let start = this.stringToDate(this.fromDateString);
    let end = this.stringToDate(this.toDateString);
    this.statsService.downloadFile(this.actionsTypeSelect, start, end, this.intervalSelect).subscribe(
      data => {
        switch (data.type) {
          case HttpEventType.Response:
            const downloadedFile = new Blob([data.body], { type: data.body.type });
            const a = document.createElement('a');
            a.setAttribute('style', 'display:none;');
            document.body.appendChild(a);
            a.download = 'file.xlsx';
            a.href = URL.createObjectURL(downloadedFile);
            a.target = '_blank';
            a.click();
            document.body.removeChild(a);
            break;
        }
      });
  }
}

class BookingsWithKeys {
  key: string;
  value: number;
}
