import { createMapper, fromValue, mapFrom } from '@automapper/core';
import { createMetadataMap, pojos } from '@automapper/pojos';
import { DeviceConfigurationUI } from '../models/device-configuration';
import { DeviceConfigurationViewModel } from './../api/models';

export const mapper = createMapper({
  name: 'vseiotMapper',
  pluginInitializer: pojos
});

export function createMetadata(): void {

  // BackEnd ViewModel
  createMetadataMap<DeviceConfigurationViewModel>('DeviceConfigurationViewModel', {
    deviceStatus: 'DeviceStatus',
    id: Number,
    name: String,
    ioTCorePort: Number,
    ioTStatus: 'IoTStatus',
    vseIpAddress: String,
    vsePort: Number,
    vseType: String
  });

  // FrontEnd
  createMetadataMap<DeviceConfigurationUI>('DeviceConfigurationUI', {
    name: String,
    vseType: String,
    vseIpAddress: String,
    vsePort: Number,
    ioTCorePort: Number,
    ioTStatus: 'IoTStatus',
    deviceStatus: 'DeviceStatus'
  });
}

createMetadata();

// FrontEnd <=> BackEnd ViewModel
mapper.createMap<DeviceConfigurationUI, DeviceConfigurationViewModel>('DeviceConfigurationUI', 'DeviceConfigurationViewModel');

// BackEnd ViewModel <=> FrontEnd
mapper.createMap<DeviceConfigurationViewModel, DeviceConfigurationUI>('DeviceConfigurationViewModel', 'DeviceConfigurationUI');