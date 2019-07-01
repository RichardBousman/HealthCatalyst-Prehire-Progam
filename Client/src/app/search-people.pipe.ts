import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'searchForPeople',
  pure: false
})
export class SearchForPeoplePipe implements PipeTransform {

  transform(pipeData, pipeModifier): any {
    return pipeData.filter(eachItem => {

      if ( eachItem['firstName'] == null || eachItem['lastName'] == null)
      {
        return false ;
      }

      if ( pipeModifier == null || pipeModifier == "" )
      {
        return true ;
      }

      return (
        eachItem['firstName'].toLowerCase().includes(pipeModifier.toLowerCase()) ||
        eachItem['lastName'].toLowerCase().includes(pipeModifier.toLowerCase())
      )
    });
  }
}
