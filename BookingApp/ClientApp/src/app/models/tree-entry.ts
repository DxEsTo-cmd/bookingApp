import { Folder } from "./folder";
import { Resource } from "./resource";
import { Logger } from "../services/logger.service";

export class TreeEntry {
  //mutual original
  id: number;
  title: string;
  isActive: boolean;
  image: Array<string>;

  //derived original
  isFolder?: boolean;
  parentFolderId?: number;

  //resource-specific
  occupancy?: number;
  occupancyTitle?: string;
  price?: number;

  constructor(original: Folder | Resource) {

    this.isFolder = (<Folder>original).parentFolderId !== undefined;

    this.id = original.id;
    this.title = original.title;
    this.isActive = original.isActive;
    this.image = original.image
    
    if (this.isFolder) {
      this.parentFolderId = (<Folder>original).parentFolderId;
    }
    else {
      this.parentFolderId = (<Resource>original).folderId;
      this.price= (<Resource>original).price;
    }

    //Logger.log(`${original.title} - ${this.isFolder}`);

  }
}
