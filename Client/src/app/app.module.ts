import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FileSelectDirective } from 'ng2-file-upload';
import { FormsModule } from '@angular/forms';

// Snackbar changes

import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {MatButtonModule, MatSnackBarModule } from "@angular/material"



import { AppComponent } from './app.component';
import {SearchForPeoplePipe} from './search-people.pipe'; 
import {PersonSimpleComponent} from './personsimplerow/PersonSimple.Component';
import {PersonDetailsComponent} from './persondetails/PersonDetails.Component' ;
import {PersonEditComponent} from './personedit/PersonEdit.Component';
import {PersonAddComponent} from './addperson/AddPerson.Component';
import { InterestsComponent } from './Interests/Interest.Component' ;
import { TechInfoComponent } from './TechInfoDialog/TechInfo.Component';


@NgModule({
  declarations: [
    AppComponent,
    PersonSimpleComponent,
    PersonDetailsComponent,
    PersonEditComponent,
    PersonAddComponent,
    InterestsComponent,
    TechInfoComponent,
    SearchForPeoplePipe,
    FileSelectDirective
  ],
  imports: [
    BrowserModule,
    FormsModule,
    BrowserAnimationsModule,  // for Snackbar
    MatButtonModule,  // for Snackbar
    MatSnackBarModule],  // for Snackbar
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
