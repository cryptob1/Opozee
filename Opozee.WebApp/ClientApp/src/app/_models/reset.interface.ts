export interface resetModel {
 
  oldpassword: string; // required, must be valid email format
  newpassword: string; // required, value must be equal to confirm password.
  confirmPassword: string; // required, value must be equal to password.
}
