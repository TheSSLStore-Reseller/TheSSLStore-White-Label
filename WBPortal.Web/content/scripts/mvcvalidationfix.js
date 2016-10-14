function enableClientValidation() {
    var allFormOptions = window.mvcClientValidationMetadata;
    if (allFormOptions) {
        while (allFormOptions.length > 0) {
            var thisFormOptions = allFormOptions.pop();
            __MVC_EnableClientValidation(thisFormOptions);
        }
    }
}