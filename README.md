# Super Web View
A drop in replacement for WebView in Xamarin Forms.

I created this control to better fulfill my own requirements for a web browsing component on Xamarin Forms. This control allows for the following features:

## Better Navigation

The original `WebView` components does not best support complicated scenarios such as asyncronously cancelling navigation to a website. I originally discovered this limitation when implementing website filtering in an app and raised [this](https://github.com/xamarin/Xamarin.Forms/pull/14137) PR to address the limitation. Unfortunately it doesn't look like it will ever make it into Forms with the blocking of breaking changes in Forms 5, as it moves to LTS.



## Better Platform Customisaton

The native platforms have some cool features which can be quite hard to tap into, this control aims to allow you to specifically override the behaviour of native api's, and easily reintegrate back into the forms control.
