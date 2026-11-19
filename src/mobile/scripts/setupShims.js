const fs = require('fs');
const path = require('path');

const rnDir = path.resolve(__dirname, '../node_modules/react-native');
if (fs.existsSync(rnDir)) {
  // 1. asset-registry shim
  const assetRegistryPath = path.join(rnDir, 'asset-registry.js');
  const assetRegistryContent = `const r = require('@react-native/assets-registry/registry');
module.exports = r;
module.exports.default = r;
module.exports.registerAsset = r.registerAsset;
module.exports.getAssetByID = r.getAssetByID;
`;
  fs.writeFileSync(assetRegistryPath, assetRegistryContent);

  // 2. unstable-internals-do-not-use shim
  const unstableInternalsPath = path.join(rnDir, 'unstable-internals-do-not-use.js');
  const unstableInternalsContent = `'use strict';
const NativeRedBox = require('./Libraries/NativeModules/specs/NativeRedBox');
const DevLoadingView = require('./Libraries/Utilities/DevLoadingView');
const NativeExceptionsManager = require('./Libraries/Core/NativeExceptionsManager');
const NativeSourceCode = require('./Libraries/NativeModules/specs/NativeSourceCode');
const AssetSourceResolver = require('./Libraries/Image/AssetSourceResolver');

const getExport = (mod) => (mod && mod.default !== undefined ? mod.default : mod);

module.exports = {
  get NativeRedBox() {
    return getExport(NativeRedBox);
  },
  get DevLoadingView() {
    return getExport(DevLoadingView);
  },
  get NativeExceptionsManager() {
    return getExport(NativeExceptionsManager);
  },
  get NativeSourceCode() {
    return getExport(NativeSourceCode);
  },
  get AssetSourceResolver() {
    return getExport(AssetSourceResolver);
  },
};
`;
  fs.writeFileSync(unstableInternalsPath, unstableInternalsContent);
}
