function initTinyMCE_Description(target) {
    const copyToMeta = $(target).data('copy-meta') === true;

    const existingEditor = tinymce.get(target.id);
    if (existingEditor) tinymce.remove(existingEditor);

    tinymce.init({
        target: target,
        height: 700,
        directionality: 'rtl',
        language: 'fa',
        menubar: 'file edit view insert format table tools help',
        branding: false,
        plugins: [
            'advlist autolink lists link image charmap preview anchor',
            'searchreplace visualblocks code fullscreen',
            'insertdatetime media table help wordcount',
            'textcolor colorpicker'
        ],
        toolbar: [
            'undo redo | formatselect | bold italic underline forecolor backcolor | alignright aligncenter alignleft alignjustify |',
            'bullist numlist outdent indent | link image table | removeformat | code fullscreen'
        ].join(' '),
        content_style: "body { font-family: Vazir, sans-serif; direction: rtl; text-align: right; font-size: 18px; }",
        setup: function (editor) {
            editor.on('init', function () {
                tinymce.triggerSave();
            });

            editor.on('change input', function () {
                tinymce.triggerSave();

                if (copyToMeta) {
                    const text = editor.getContent({ format: 'text' });
                    $('#MetaDescription').val(text);
                    $('#MetaDescription').valid();
                }

                // validation Razor
                $('#' + target.id).valid();
            });
        }
    });
}

function initTinyMCE_Specifications(target) {
    const existingEditor = tinymce.get(target.id);
    if (existingEditor) tinymce.remove(existingEditor);

    tinymce.init({
        target: target,
        height: 700,
        directionality: 'rtl',
        language: 'fa',
        menubar: 'file edit view insert format table tools help',
        branding: false,
        plugins: [
            'lists table textcolor colorpicker',
            'wordcount fullscreen preview'
        ],
        toolbar: 'undo redo | bold italic underline forecolor backcolor | bullist numlist | outdent indent | table',
        table_default_attributes: { border: '1' },
        table_default_styles: { width: '100%', borderCollapse: 'collapse' },
        content_style: `
            body {
                font-family: Vazir, sans-serif;
                direction: rtl;
                text-align: right;
                font-size: 18px;
            }
            table {
                border-collapse: collapse;
                width: 100%;
            }
            table, th, td {
                border: 1px solid #ccc;
                padding: 6px;
            }
        `,
        setup: function (editor) {
            // وقتی editor mount شد، textarea اصلی را sync کن
            editor.on('init', function () {
                tinymce.triggerSave();
            });

            // وقتی محتوا تغییر کرد
            editor.on('change input', function () {
                tinymce.triggerSave(); // sync مقدار textarea اصلی
                $('#' + target.id).valid(); // validation Razor
            });
        }
    });
}


// init همه textarea ها در یک context (document یا modal)
function initTinyMCE_Auto(context) {
    context = context || $(document);

    context.find('#descEditor').each(function () {
        initTinyMCE_Description(this);
    });

    context.find('#specEditor').each(function () {
        initTinyMCE_Specifications(this);
    });

}

// وقتی صفحه load شد
$(document).ready(function () {
    initTinyMCE_Auto();
});

// وقتی مودال باز شد
$(document).on('shown.bs.modal', function (e) {
    initTinyMCE_Auto($(e.target));
});