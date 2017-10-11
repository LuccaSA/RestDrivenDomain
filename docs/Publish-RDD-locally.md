If you want to avoid the Nuget roundtrip each time you want to modify RDD source code while working on a client application, you can publish RDD packages locally on your computer.

Once you are happy with your modifications, you still need to publish RDD to Nuget, wait for a release version, and reference it from your client application. But at least, you have to do it only once!

1. Create a local directory where dotnet will publish your packages

![](https://user-images.githubusercontent.com/2686141/31424219-5d02585c-ae59-11e7-9336-5353abdde907.png)

2. Increase RDD projects version in Visual Studio project properties panel

![](https://user-images.githubusercontent.com/2686141/31424229-637599e2-ae59-11e7-9a08-65b0b8fd3ec4.png)

3. Build RDD projects and tell dotnet to publish them into your local directory. It might be a good idea to publish them into the right order, Domain first, then Infra, Application & Web.

![](https://user-images.githubusercontent.com/2686141/31424352-fe34adc4-ae59-11e7-8cd2-f620cbf5d413.png)

4. Now from your client application, open the "Managed Nuget packages for Solution" panel and add your local directory as a source of NUget packages

![](https://user-images.githubusercontent.com/2686141/31424506-ee26b408-ae5a-11e7-9b79-cd9952edf131.png)

5. Run the Update-Package from the command line, not from the panel

![](https://user-images.githubusercontent.com/2686141/31424388-31760390-ae5a-11e7-827d-3f1f87925cec.png)

You are done! Re-run that process each time you make a modification to a RDD project. Step 4. is a one time task. Pay attention to which RDD project you modify, you will need to publish all its RDD dependencies along.
