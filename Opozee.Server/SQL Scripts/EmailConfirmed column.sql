ALTER TABLE [dbo].[Users] ADD EmailConfirmed BIT

--For existing users set EmailConfirmed to true 
--UPDATE [dbo].[Users] SET EmailConfirmed=1