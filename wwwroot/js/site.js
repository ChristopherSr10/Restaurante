document.getElementById("platilloForm").addEventListener("input", function () {
    var nombrePlatillo = document.getElementById("nombrePlatillo").value.trim();
    var precioPlatillo = document.getElementById("precioPlatillo").value.trim();
    var descripcionPlatillo = document.getElementById("descripcionPlatillo").value.trim();
    var numeroIngredientes = parseInt(document.getElementById("numeroIngredientes").value);

    var crearIngredientesBtnContainer = document.getElementById("crearIngredientesBtnContainer");
    var guardarBtnContainer = document.getElementById("guardarBtnContainer");

    if (nombrePlatillo !== "" && precioPlatillo !== "" && descripcionPlatillo !== "" && !isNaN(numeroIngredientes) && numeroIngredientes > 0) {
        crearIngredientesBtnContainer.style.display = "";
        guardarBtnContainer.style.display = "none";
    } else {
        crearIngredientesBtnContainer.style.display = "none";
        guardarBtnContainer.style.display = "none";
    }
});

function crearFormularios() {
    var numeroIngredientes = parseInt(document.getElementById("numeroIngredientes").value);
    var container = document.getElementById("ingredientes-container");
    container.innerHTML = "";

    for (var i = 0; i < numeroIngredientes; i++) {
        var formGroup1 = document.createElement("div");
        formGroup1.className = "form-group";
        var label1 = document.createElement("label");
        label1.innerText = "Nombre Ingrediente:";
        var input1 = document.createElement("input");
        input1.type = "text";
        input1.name = "Ingredientes[" + i + "].nombreIngrediente";
        input1.className = "form-control";
        formGroup1.appendChild(label1);
        formGroup1.appendChild(input1);

        var formGroup2 = document.createElement("div");
        formGroup2.className = "form-group";
        var label2 = document.createElement("label");
        label2.innerText = "Cantidad Disponible (porciones para hacer un platillo):";
        var input2 = document.createElement("input");
        input2.type = "text";
        input2.name = "Ingredientes[" + i + "].cantidadDisponible";
        input2.className = "form-control";
        formGroup2.appendChild(label2);
        formGroup2.appendChild(input2);

        container.appendChild(formGroup1);
        container.appendChild(formGroup2);
    }

    document.getElementById("crearIngredientesBtnContainer").style.display = "none";
    document.getElementById("guardarBtnContainer").style.display = "";
}

var ingredientInputs = document.querySelectorAll('.ingredient-input');

// Agregar evento de clic a cada campo de ingrediente
ingredientInputs.forEach(function (input) {
    input.addEventListener('click', function () {
        // Verificar si el campo ya está habilitado
        if (!this.readOnly) {
            return;
        }

        // Verificar si hay algún campo habilitado
        var hasEditableField = Array.from(ingredientInputs).some(function (i) {
            return !i.readOnly;
        });

        // Si hay algún campo habilitado, mostrar advertencia
        if (hasEditableField) {
            document.getElementById('editWarning').style.display = 'block';
            return;
        }

        // Deshabilitar todos los campos excepto el campo clicado
        ingredientInputs.forEach(function (i) {
            i.readOnly = true;
            i.classList.remove('editable');
        });

        this.readOnly = false; // Habilitar el campo clicado
        this.classList.add('editable');

        // Ocultar la advertencia
        document.getElementById('editWarning').style.display = 'none';
    });
});

