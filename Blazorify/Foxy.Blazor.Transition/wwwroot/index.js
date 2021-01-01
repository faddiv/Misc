// This file is to show how a library package may provide JavaScript interop features
// wrapped in a .NET API
window.Foxy = (function (foxy) {

  var functionHandlers = new Map();
  var increment = 0;
  /**
   *
   * @param {HTMLElement} element
   * @param {string} eventName
   * @param {any} blazorTarget
   * @param {string} method
   * @param {boolean} once
   * @param {object} once
   * @returns {number} function handle
  */
  function addBlazorEventListener(element, eventName, blazorTarget, method, once) {
    var func = function () {
      blazorTarget.invokeMethodAsync(method);
      if (once) {
        element.removeEventListener(eventName, func);
      }
    }
    element.addEventListener(eventName, func, false);
    var key = ++increment;
    functionHandlers.set(key, func);
    return key;
  }
  foxy.addBlazorEventListener = addBlazorEventListener;
  /**
   *
   * @param {HTMLElement} element
   * @param {string} eventName
   * @param {number} functionHandle
   */
  function removeBlazorEventListener(element, eventName, functionHandle) {
    var func = functionHandlers.get(functionHandle);
    if (func) {
      element.removeEventListener(eventName, func);
      functionHandlers.delete(functionHandle);
    } else {
      console.warn("Function with handle already removed.", functionHandle);
    }
  }
  foxy.removeBlazorEventListener = removeBlazorEventListener;
  /*foxy.Blazor = (function (blazor) {
    blazor.Transition = (function (transition) {



    })(blazor.Transition || {});
  })(foxy.Blazor || {});*/
  return foxy;
})(window.Foxy || {});
