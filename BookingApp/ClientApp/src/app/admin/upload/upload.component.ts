import { Component, OnInit, Output, Input, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { UploadDto } from './UploadDto/UploadDto';
import { FolderService } from '../../services/folder.service';
import { Folder } from '../../models/folder';

@Component({
  selector: 'app-upload',
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.css']
})
export class UploadComponent implements OnInit {

  private counter: number = 1; // How much position for upload image show for user
  private maxCountImage: number = 5; // How much images you can load. Default value 5
  private items: Array<UploadDto>; // Array which contain all image and empty field(if it exist)

  /* Input prameters */
    // Change the value maxCountImage
    @Input()
    set MaxCountImage(maxCountImage: number) {
      if (maxCountImage > 0) {
        this.maxCountImage = maxCountImage;
      }
    }
    
    @Input() set InputImages(inputImagesComp: any) {
      let readyImage: Array<UploadDto> = [];
      if(inputImagesComp instanceof Array)
      {
          inputImagesComp.forEach(element => {
            let uploadDto = new UploadDto(null);
            uploadDto.imageUrl = element;
            uploadDto.ready = true;
            readyImage.push(uploadDto)
          });
      }
      else
      {
        if(inputImagesComp != null && inputImagesComp.length > 0) {   
          let uploadDto = new UploadDto(null);
          uploadDto.imageUrl = inputImagesComp;
          uploadDto.ready = true;
          readyImage.push(uploadDto)
        }
      }
      readyImage.push(new UploadDto(new FormData()));
      this.items = readyImage;
      let index : number;
      if(this.items.length > this.maxCountImage) {
        this.items.forEach(element => {
          if(!element.ready)
            index = this.items.indexOf(element);
            this.items = this.items.splice(index-1,1);
        });
      }
      this.imageForUpload.emit(this.items);
    }
  /* Input parameters */ 

  // Array which will be upload from the upload.component
  @Output() imageForUpload: EventEmitter<UploadDto[]> = new EventEmitter<UploadDto[]>();

  constructor(private folderService: FolderService,private http: HttpClient) {
    this.items = [new UploadDto(new FormData())]
   }

  ngOnInit() {
    console.log()
  }

  public UploadPhoto(files, item: UploadDto): void {
     if (files.length === 0) {
       return;
     }

     const file = <File>files[0];

     const formData = new FormData();
     //formData.append('file', file, file.name);

     for (let file of files)
      formData.append(file.name, file);

     const index = this.items.indexOf(item);
    
     this.items[index].file = formData;

     this.items[index].ready = true;
    //  this.counter === this.items.length  &&
     if (this.maxCountImage >= this.items.length) {
        var reader = new FileReader();
        this.items[index].imagePath = files;
         reader.readAsDataURL(files[0]); 
         reader.onload = (_event) => { 
         this.items[index].imageUrl = reader.result;
        }
        if (this.maxCountImage > this.items.length) {
          this.items.push(new UploadDto(new FormData()));
          this.counter++;
        }
     }
     this.UpdateOutputParameters();
  }

  public DeletePhoto(item: UploadDto): void {
     const index = this.items.indexOf(item);

     if (index >= 0) {
        this.items.splice(index, 1);
        this.counter--;
        if(this.items.length < 1)
        {
          this.items.push(new UploadDto(new FormData()));
          this.counter++;
        }
        else if( this.items[this.items.length - 1].ready) {
          this.items.push(new UploadDto(new FormData()));
          this.counter++;
        }
     }
     this.UpdateOutputParameters();
  }

  public UpdateOutputParameters() {
    let images: UploadDto[] = [];
    this.items.forEach(element => {
       if (element.ready) {
         images.push(element);
       }
    });
    this.imageForUpload.emit(images);
  }

}
