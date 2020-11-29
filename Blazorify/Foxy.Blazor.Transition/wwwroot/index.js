// This file is to show how a library package may provide JavaScript interop features
// wrapped in a .NET API
window.Foxy = (function (foxy) {

  /**
   *
   * @param {HTMLElement} element
   * @param {string} eventName
   * @param {any} blazorTarget
   * @param {string} method
   * @param {boolean} once
   * @param {object} once
  */
  function addBlazorEventListener(element, eventName, blazorTarget, method, once) {
    var func = function () {
      blazorTarget.invokeMethodAsync(method);
      if (once) {
        element.removeEventListener(eventName, func);
      }
    }
    element.addEventListener(eventName, func, false);
    return func;
  }
  foxy.addBlazorEventListener = addBlazorEventListener;
  /**
   *
   * @param {HTMLElement} element
   * @param {string} eventName
   * @param {any} func
   */
  function removeBlazorEventListener(element, eventName, func) {
    element.removeEventListener(eventName, func);
  }
  foxy.removeBlazorEventListener = removeBlazorEventListener;
  /*foxy.Blazor = (function (blazor) {
    blazor.Transition = (function (transition) {



    })(blazor.Transition || {});
  })(foxy.Blazor || {});*/
  return foxy;
})(window.Foxy || {});
