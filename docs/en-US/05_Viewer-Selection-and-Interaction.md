# 05 Viewer, Selection and Interaction

`OcctEngine` owns the interactive AIS/Viewer side of the bridge.

## Scene objects

Displayed objects have stable managed IDs within an engine owner. The API supports shape registration/display, visibility, naming, appearance, transforms, polygon offsets, annotations and deletion. Object collections are exposed through bulk-backed managed views.

## Camera and view

Camera state is represented by managed data such as eye, center, up direction and scale. The API supports standard view changes, fit operations and explicit camera state transfer.

## Selection

Selection is modeled as a structured result rather than only a selected-count/index pair. Public APIs expose selected objects and richer hit information where available. Demo/application code should use current `SelectedObjects` APIs rather than removed compatibility aliases.

## Host interaction

WinForms, WPF and Avalonia hosts adapt mouse and window events to the engine. Host code may implement rotate, pan, zoom, hover and rectangle selection policies while the reusable Bridge owns the actual OCCT viewer state.

## Thread affinity

Viewer and host lifetime follows the creating Windows UI thread/HWND. Do not treat an `OcctEngine` instance as a freely concurrent service object.

Exact camera, selection, hit-test and host members are listed in the generated API Reference.