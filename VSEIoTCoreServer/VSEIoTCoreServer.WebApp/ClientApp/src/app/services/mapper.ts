import { createMapper, fromValue, mapFrom } from '@automapper/core';
import { createMetadataMap, pojos } from '@automapper/pojos';
import { AddDeviceUI } from '../models/add-device';
import { DeviceConfigurationUI } from '../models/device-configuration';
import { AddDeviceViewModel, DeviceConfigurationViewModel } from './../api/models';

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

  createMetadataMap<AddDeviceViewModel>('AddDeviceViewModel', {
    ioTCorePort: Number,
    name: String,
    vseIpAddress: String,
    vsePort: Number
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

  createMetadataMap<AddDeviceUI>('AddDeviceUI', {
    name: String,
    vseIpAddress: String,
    vsePort: Number,
    ioTCorePort: Number,
    onboardStatus: Boolean
  });
}

createMetadata();

// FrontEnd <=> BackEnd ViewModel
mapper.createMap<DeviceConfigurationUI, DeviceConfigurationViewModel>('DeviceConfigurationUI', 'DeviceConfigurationViewModel');
mapper.createMap<AddDeviceUI, AddDeviceViewModel>('AddDeviceUI', 'AddDeviceViewModel');

// BackEnd ViewModel <=> FrontEnd
mapper.createMap<DeviceConfigurationViewModel, DeviceConfigurationUI>('DeviceConfigurationViewModel', 'DeviceConfigurationUI');
mapper.createMap<AddDeviceViewModel, AddDeviceUI>('AddDeviceViewModel', 'AddDeviceUI');