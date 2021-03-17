#import <Foundation/Foundation.h>
#include "IUnityInterface.h"
#include "UnityAppController.h"

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityARKitXRPlugin_PluginLoad(IUnityInterfaces* unityInterfaces);
extern void UnityARKit_SetRootView(UIView* view);

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityARKit_EnsureRootViewIsSetup()
{
    UnityARKit_SetRootView(_UnityAppController.rootView);
    //NSLog(@"UnityARKit_EnsureRootViewIsSetup() done.");
}

/**
void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityARKitXRPlugin_PluginLoad2(IUnityInterfaces* unityInterfaces)
{
    NSLog(@"UnityARKitXRPlugin_PluginLoad2() invoked with unityInterfaces*=%x.", unityInterfaces);
}
*/

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityARKit_RegisterPlugin()
{
    UnityRegisterRenderingPluginV5(UnityARKitXRPlugin_PluginLoad, NULL);
    //NSLog(@"UnityARKit_RegisterPlugin() done.");
}

@interface UnityARKit : NSObject

+ (void)loadPlugin;

@end

@implementation UnityARKit

+ (void)loadPlugin
{
    //NSLog(@"UnityARKit-loadPlugin started.");
    
    // This registers our plugin with Unity
    UnityRegisterRenderingPluginV5(UnityARKitXRPlugin_PluginLoad, NULL);

    // This sets up some data our plugin will need later
    UnityARKit_EnsureRootViewIsSetup();
    
    //NSLog(@"UnityARKit-loadPlugin finished.");
}

@end
