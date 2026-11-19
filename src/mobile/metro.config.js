// Learn more https://docs.expo.io/guides/customizing-metro
const { getDefaultConfig } = require('expo/metro-config');
const path = require('path');

const config = getDefaultConfig(__dirname);

const assetRegistryPath = path.resolve(__dirname, 'node_modules/react-native/asset-registry.js');
const unstableInternalsPath = path.resolve(__dirname, 'node_modules/react-native/unstable-internals-do-not-use.js');

config.resolver.extraNodeModules = {
  ...(config.resolver.extraNodeModules || {}),
  'react-native/asset-registry': assetRegistryPath,
  'react-native/unstable-internals-do-not-use': unstableInternalsPath,
};

const defaultResolveRequest = config.resolver.resolveRequest;

config.resolver.resolveRequest = (context, moduleName, platform) => {
  if (moduleName === 'react-native/asset-registry') {
    return {
      filePath: assetRegistryPath,
      type: 'sourceFile',
    };
  }
  if (moduleName === 'react-native/unstable-internals-do-not-use') {
    return {
      filePath: unstableInternalsPath,
      type: 'sourceFile',
    };
  }
  if (defaultResolveRequest) {
    return defaultResolveRequest(context, moduleName, platform);
  }
  return context.resolveRequest(context, moduleName, platform);
};

module.exports = config;
