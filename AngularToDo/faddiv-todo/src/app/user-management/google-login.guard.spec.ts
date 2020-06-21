import { TestBed } from '@angular/core/testing';

import { GoogleLoginGuard } from './google-login.guard';

describe('GoogleLoginGuard', () => {
  let guard: GoogleLoginGuard;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    guard = TestBed.inject(GoogleLoginGuard);
  });

  it('should be created', () => {
    expect(guard).toBeTruthy();
  });
});
