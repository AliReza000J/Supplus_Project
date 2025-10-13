function initTinyMCE_Description() {
    tinymce.init({
        selector: '#descEditor',
        height: 400,
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
        table_default_attributes: { border: '1' },
        table_default_styles: { width: '100%', borderCollapse: 'collapse' },
        content_style: `
            body {
                font-family: Vazir, sans-serif;
                direction: rtl;
                text-align: right;
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
            editor.on('change', function () {
                tinymce.triggerSave();
            });
        }
    });
}

function initTinyMCE_Specifications() {
    tinymce.init({
        selector: '#specEditor',
        height: 300,
        directionality: 'rtl',
        language: 'fa',
        menubar: false,
        branding: false,
        plugins: 'lists table textcolor colorpicker',
        toolbar: 'undo redo | bold italic underline forecolor backcolor | bullist numlist | table',
        table_default_attributes: { border: '1' },
        table_default_styles: { width: '100%', borderCollapse: 'collapse' },
        content_style: `
            body {
                font-family: Vazir, sans-serif;
                direction: rtl;
                text-align: right;
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
            editor.on('change', function () {
                tinymce.triggerSave();
            });
        }
    });
}

$(document).on('shown.bs.modal', function () {
    if (tinymce.editors.length > 0) {
        tinymce.remove();
    }

    if ($('#descEditor').length) initTinyMCE_Description();
    if ($('#specEditor').length) initTinyMCE_Specifications();
});
