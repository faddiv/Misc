(function (exports, Popper) {

  var popperInstance;
  /**
   * 
   * @param {HTMLElement} reference
  * @param {HTMLElement} popper
   * @param {any} options
   */
  function Popper_createPopper(reference, popper, options) {
    if (popperInstance) {
      popperInstance.destroy();
    }
    for (var prop in options) {
      if (options.hasOwnProperty(prop) && options[prop] === null) {
        delete options[prop];
      }
    }
    popperInstance = Popper.createPopper(reference, popper, options);
  }

  function Popper_update() {
    if (popperInstance) {
      popperInstance.update();
    }
  }

  exports.Popper_createPopper = Popper_createPopper;
  exports.Popper_update = Popper_update;

  /**
   * 
   * @param {HTMLElement} element
   */
  exports.reflow = function reflow(element) {
    element.offsetHeight;
  }
  var idx = 0;
  /**
   * 
   * @param {HTMLElement} element
   */
  exports.getElHeight = function getElHeight(element) {
    return element.getBoundingClientRect().height;
  }

  /**
   *
   * @param {HTMLElement} element
   */
  exports.getElScrollSize = function getElHeight(element) {
    return element.scrollHeight;
  }

})(window, Popper);
