import { Component, OnInit } from '@angular/core';
import { ChatService } from '../../services/chat.service';
import { ActivatedRoute } from '@angular/router';
import { Chat } from '../../models/chat';

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent implements OnInit {

  constructor(private chatService: ChatService,  private actRoute: ActivatedRoute) { }
  
  id: string;
  chats: Array<Chat> = [];

  ngOnInit() {
    this.id = this.actRoute.snapshot.params['id'];
    this.chatService.getChats(this.id).subscribe((respornse) => {
      this.chats = respornse;
    })
  }

  goToChat(resourceId: number) {
    //window.open('http://google.com','Window','width=600,height=300');
    window.open('http://localhost:4200/?userId='+ this.id+'&resourceId=' + resourceId,'MyWindow','width=600,height=700');

    //window.open('http://www.google.com/{chat.resourceId}','MyWindow','width=600,height=300')
  }
}
