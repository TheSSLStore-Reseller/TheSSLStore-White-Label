        $(function () {
            setFocusOnFirst();
            applyjqcolors();
        });

        function styleup() {
            var is_chrome = navigator.userAgent.toLowerCase().indexOf('chrome') > -1;
            $("select,fieldset,input:text,.ae-lookup-multidisplay").addClass("ui-corner-all");
            $("input:text,.ae-lookup-multidisplay").addClass('ui-widget-content');
            $("input[type=submit]").addClass("abtn");
            $(".atbl thead").addClass("ui-state-default");
            
            $(".ae-lookup-list > li:even, .ae-lookup-list li:odd").removeClass('ui-widget-content ui-state-highlight');
            $(".ae-lookup-list > li:even").addClass("ui-widget-content"); 
            $(".ae-lookup-list > li:odd").addClass("ui-state-highlight");

            $(".atbl tbody tr:even, tbody tr:odd").removeClass("ui-widget-content ui-state-highlight");
            $(".atbl tbody tr:even").addClass("ui-widget-content");
            $(".atbl tbody tr:odd").addClass("ui-state-highlight");            
           
            $('.ui-state-highlight a').css('color', $('.ui-state-default').css('color'));
            $ae.mybutton(".abtn");
            $ae.mybutton(".ae-lookup-morebtn");
            $(".field-validation-error").addClass('ui-state-error ui-corner-all');

            if (!is_chrome)
                $(".input-validation-error").addClass('ui-state-error');            
        }

        function applyjqcolors() {
            $.fx.speeds._default = 300;            
            $(window).bind("resize", function (e) { $("#main-container").css("min-height", ($(window).height() - 120) + "px"); });
            $("#main-container").css("min-height", ($(window).height() - 120) + "px").addClass("ui-widget-content");

            styleup();
            $(document).ajaxComplete(styleup);
            $(document).bind("awesome", styleup);
            $(document).bind('dialogopen', function () { $('.ui-dialog').addClass('transparent'); });
        }

        function setFocusOnFirst() {
            $("input:text:visible:first").focus();
        }