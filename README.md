PlayReady.WindowsStore.Proactive.Sample
=======================================

A modified sample to properly implement and prove pro-active license acquisition for a locally stored file. Modified from http://code.msdn.microsoft.com/windowsapps/PlayReady-sample-for-bb3065e7#content


The changes necessary to implement the pro-active license and playback of the local file are in [b0a52323f2](https://github.com/PeteGoo/PlayReady.WindowsStore.Proactive.Sample/commit/b0a52323f271a1f605db2365514db38b562061bf)

These include:

* Using the persistent license url as defined at [the test rights server custom rights page](http://playready.directtaps.net/pr/doc/customrights/)
* Changing the content id (KeyStringId) to match the id from the video header
* Adding the code to open a local file stream instead of a URL.
