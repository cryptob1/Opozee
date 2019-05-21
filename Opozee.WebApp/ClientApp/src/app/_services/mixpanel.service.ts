import { Injectable } from '@angular/core';
import * as mixpanel from 'mixpanel-browser';

@Injectable()
export class MixpanelService {

  /**
   * Initialize mixpanel.
   *
   * @param {string} userToken
   * @memberof MixpanelService
   */
  init(userToken: string): void {
    mixpanel.init('410cd4187cb6b07c074955910e753010');
    mixpanel.identify(userToken);
  }

  /**
   * Push new action to mixpanel.
   *
   * @param {string} id Name of the action to track.
   * @param {*} [action={}] Actions object with custom properties.
   * @memberof MixpanelService
   */
  track(id: string, action: any = {}): void {
    mixpanel.track(id, action);
  }
}
