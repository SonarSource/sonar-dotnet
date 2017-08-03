Remove-Item .\expected -Recurse -Force
Rename-Item .\actual .\expected
Remove-Item .\expected\.git -Recurse -Force