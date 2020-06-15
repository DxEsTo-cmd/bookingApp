import { Component, OnInit } from '@angular/core';
import { TreeEntry } from '../../models/tree-entry';
import { ResourceService } from '../../services/resource.service';
import { FolderService } from '../../services/folder.service';
import { AuthService } from '../../services/auth.service';
import { Folder } from '../../models/folder';
import { Resource } from '../../models/resource';
import { TreeNode } from '../../models/tree-node';
import { NgxSpinnerService } from 'ngx-spinner';

@Component({
  selector: 'app-flat',
  templateUrl: './flat.component.html',
  styleUrls: ['./flat.component.css']
})
export class FlatComponent implements OnInit {

  resourceEntries: TreeEntry[];
  folderEntries: TreeEntry[];
  allEntries: TreeEntry[];
  treeFlat: TreeNode[];
  treeRoot: TreeNode;
  barrierCount : number;

  authChangedSubscription: any;
  constructor( private resourceService: ResourceService,
    private folderService: FolderService,
    private authService: AuthService,
    private spinner: NgxSpinnerService) { }

  ngOnInit() {
    this.spinner.show();
    this.resetdata();
    this.authChangedSubscription = this.authService.AuthChanged.subscribe(() => this.resetdata());
  }

  ngOnDestroy() {
    this.authChangedSubscription.unsubscribe();
  };

  resetdata() {
    this.resourceEntries = [];
    this.folderEntries = [];
    this.allEntries = [];
    this.treeFlat = [];
    this.treeRoot = new TreeNode(0, "[root]", new TreeEntry(this.folderService.newRoot()), []);
    this.barrierCount = 0;

    this.folderService.getList().subscribe((result: Folder[]) => {
      for (let key in result) {
        let entry = new TreeEntry(result[key]);
        this.folderEntries[key] = entry;
        this.allEntries.push(entry);
      }
      this.reachDoubleBarrier();
    });

    this.resourceService.getResources().subscribe((result: Resource[]) => {
      for (let key in result) {
        let entry = new TreeEntry(result[key]);
        this.resourceEntries[key] = entry;
        this.allEntries.push(entry);
      }
      this.resourceService.resetOccupancies(this.resourceEntries);
      this.reachDoubleBarrier();
      setTimeout(() => {
        /** spinner ends after 5 seconds */
        this.spinner.hide();
      }, 3000); 
    });
  }
    
  getResourcesByFolderId(folderId: number): Array<TreeEntry> {
      let resources : Array<TreeEntry> = [];
      this.resourceEntries.forEach(element => {
        if(element.parentFolderId == folderId && element.isActive)
            resources.push(element);
      });
      return resources;
  }

  reachDoubleBarrier() {
    this.barrierCount++;

    if (this.barrierCount == 2) {
      this.buildTree();
    }
  }

  buildTree() {
    //sort everyting by id
    this.allEntries.sort(function (a, b) { return a.id - b.id });

    //nodify everything and stick into the root
    let folderNodeMap: TreeNode[] = [];
    let allNodeMap: TreeNode[] = [];
        
    this.treeRoot.children = [];
    for (let entry of this.allEntries) {

      let node = new TreeNode(0, (entry.isFolder ? `[${entry.title}]` : entry.title), entry, []);

      if (entry.isFolder === true)
        folderNodeMap[entry.id] = node;

      allNodeMap.push(node);

      this.treeRoot.children.push(node);
    }

    //stick the nodes on
    for (let currentNode of allNodeMap) {
      let targetParent = currentNode.item.parentFolderId;

      if (targetParent != null && folderNodeMap[targetParent] != undefined) {
        folderNodeMap[targetParent].children.push(currentNode);
        this.treeRoot.children.splice(this.treeRoot.children.indexOf(currentNode),1);       
      }
    }

    //resursively fill nesting and form flat list of nodes
    this.nestAndFlatten(this.treeRoot);
  }

  nestAndFlatten(pNode: TreeNode) {
    this.treeFlat.push(pNode);
    for (let cNode of pNode.children) {
      cNode.nesting = pNode.nesting + 1;
      this.nestAndFlatten(cNode);
    }
  }
}
