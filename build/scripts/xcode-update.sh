#!/bin/bash
# https://github.com/PrismLibrary/Prism/blob/master/build/scripts/xcode-update.sh
echo "Setting Xcode Override to: $xcodeRoot"
xcodeRoot=/Applications/Xcode_$xcodeOverride.app


echo '##vso[task.setvariable variable=MD_APPLE_SDK_ROOT;]'$xcodeRoot
sudo chmod xcode-select --switch $xcodeRoot/Contents/Developer

# Apply temporary workaround for https://github.com/actions/virtual-environments/issues/1932
rm -f ${HOME}/Library/Preferences/Xamarin/Settings.plist