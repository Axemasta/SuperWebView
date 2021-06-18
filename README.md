# Super Web View
A drop in replacement for WebView in Xamarin Forms.

I created this control to better fulfill my own requirements for a web browsing component on Xamarin Forms. This control is a direct fork of the Xamarin control, I have extensively used code form the Xamarin Forms repository to create this library. Unfortunately due to encapsulation certain things required large chunks of code to be pulled out of Xamarin in order to get this control to function exactly like its Forms counterpart.

![Sample App Using SuperWebView](docs/assets/SampleScreenshot.png)

## Build Status

|                       | Build Status                                                                                                                                                                                                                                                              |
|-----------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Axemasta.SuperWebView | [![Build Status](https://axemasta.visualstudio.com/GitHub%20Pipelines/_apis/build/status/Axemasta.SuperWebView?branchName=refs%2Fpull%2F1%2Fmerge)](https://axemasta.visualstudio.com/GitHub%20Pipelines/_build/latest?definitionId=4&branchName=refs%2Fpull%2F1%2Fmerge) |

## Better Navigation

The original `WebView` components does not best support complicated scenarios such as asyncronously cancelling navigation to a website. I originally discovered this limitation when implementing website filtering in an app and raised [this](https://github.com/xamarin/Xamarin.Forms/pull/14137) PR to address the limitation. Unfortunately it doesn't look like it will ever make it into Forms with the blocking of breaking changes in Forms 5, as it moves to LTS.

Using the new `SuperWebNavigatingArgs` it is possible to defer navigation until you have confirmed whether a url can be accessed:

```csharp
async void OnNavigating(object sender, SuperWebNavigatingEventArgs e)
{
    if (e.CanCancel)
    {
        var token = e.GetDeferral();

        bool canBrowse = await CanBrowse(e.Url);

        if (!canBrowse)
        {
            e.Cancel();
        }

        token.Complete();
    }
}
```

## Better Platform Customisaton

The native platforms have some cool features which can be quite hard to tap into, this control aims to allow you to specifically override the behaviour of native api's, and easily reintegrate back into the forms control.

## NuGet Package

This library is available on NuGet.org, you should install it into all of your Forms projects (shared, ios, android).

| Package                                    | NuGet                                                        |
| ------------------------------------------ | ------------------------------------------------------------ |
| [Axemasta.SuperWebView][SuperWebViewNuGet] | [![SuperWebViewNuGetShield]][SuperWebViewNuGet]|

## Sample App

This project comes with a fully implemented sample app to demonstrate all of the features of this control. I will be updating the wiki with all of the current possibilities but for now please run the sample app to get a flavour of whats possible. Nothing in the sample app requires custom renderers and is available out of the box!

![Preview of the sample app](docs/assets/SampleVideo.gif)

[SuperWebViewNuGet]: https://www.nuget.org/packages/Axemasta.SuperWebView/
[SuperWebViewNuGetShield]: https://img.shields.io/nuget/v/Axemasta.SuperWebView.svg