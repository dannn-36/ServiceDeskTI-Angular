// Datos del usuario actual
const currentUser = {
    name: 'Carlos Mendoza',
    email: 'carlos.mendoza@empresa.com',
    department: 'Ventas',
    location: 'piso2'
};

// Tickets del usuario
let myTickets = [
    {
        id: 'TK-045',
        category: 'hardware',
        title: 'Mi computadora no enciende',
        description: 'Desde esta mañana mi computadora no enciende. Ayer funcionaba perfectamente.',
        status: 'en-progreso',
        priority: 'alta',
        created: '2024-01-15',
        location: 'piso2',
        assignedTo: 'Ana López',
        messages: [
            {
                type: 'user',
                author: 'Carlos Mendoza',
                time: '09:15',
                content: 'Hola, mi computadora no enciende desde esta mañana. ¿Pueden ayudarme?'
            },
            {
                type: 'agent',
                author: 'Ana López',
                time: '09:30',
                content: 'Hola Carlos, voy a revisar tu caso. ¿Podrías verificar si el cable de poder está bien conectado?'
            },
            {
                type: 'user',
                author: 'Carlos Mendoza',
                time: '09:35',
                content: 'Sí, está conectado. Las luces del monitor tampoco encienden.'
            },
            {
                type: 'agent',
                author: 'Ana López',
                time: '10:15',
                content: 'Perfecto, voy a ir a tu escritorio en 15 minutos para revisar el equipo directamente.'
            }
        ]
    },
    {
        id: 'TK-042',
        category: 'software',
        title: 'No puedo abrir Excel',
        description: 'Excel se cierra automáticamente cuando trato de abrir cualquier archivo.',
        status: 'resuelto',
        priority: 'media',
        created: '2024-01-12',
        location: 'piso2',
        assignedTo: 'Juan Pérez',
        messages: [
            {
                type: 'user',
                author: 'Carlos Mendoza',
                time: '14:20',
                content: 'Excel se cierra solo cuando abro archivos. ¿Pueden ayudarme?'
            },
            {
                type: 'agent',
                author: 'Juan Pérez',
                time: '14:45',
                content: 'Hola Carlos, voy a reinstalar Office en tu computadora. Te aviso cuando esté listo.'
            },
            {
                type: 'agent',
                author: 'Juan Pérez',
                time: '16:30',
                content: 'Listo! He reinstalado Office y ya debería funcionar correctamente. ¿Puedes probarlo?'
            },
            {
                type: 'user',
                author: 'Carlos Mendoza',
                time: '16:45',
                content: '¡Perfecto! Ya funciona bien. Muchas gracias por la ayuda.'
            }
        ]
    },
    {
        id: 'TK-038',
        category: 'network',
        title: 'Internet muy lento',
        description: 'La conexión a internet está muy lenta, especialmente para descargar archivos.',
        status: 'abierto',
        priority: 'baja',
        created: '2024-01-10',
        location: 'piso2',
        assignedTo: 'Pendiente',
        messages: [
            {
                type: 'user',
                author: 'Carlos Mendoza',
                time: '11:30',
                content: 'El internet está muy lento hoy. ¿Hay algún problema con la red?'
            }
        ]
    }
];

let selectedTicket = null;

function getStatusBadge(status) {
    const statusConfig = {
        'abierto': { color: 'bg-blue-100 text-blue-800', text: 'Abierto', icon: '🔵' },
        'en-progreso': { color: 'bg-yellow-100 text-yellow-800', text: 'En Progreso', icon: '🟡' },
        'resuelto': { color: 'bg-green-100 text-green-800', text: 'Resuelto', icon: '✅' },
        'cerrado': { color: 'bg-gray-100 text-gray-800', text: 'Cerrado', icon: '⚫' }
    };
    return statusConfig[status] || statusConfig['abierto'];
}

function getPriorityBadge(priority) {
    const priorityConfig = {
        'baja': { color: 'bg-gray-100 text-gray-800', text: 'Baja', icon: '🟢' },
        'media': { color: 'bg-blue-100 text-blue-800', text: 'Media', icon: '🟡' },
        'alta': { color: 'bg-orange-100 text-orange-800', text: 'Alta', icon: '🟠' },
        'urgente': { color: 'bg-red-100 text-red-800', text: 'Urgente', icon: '🔴' }
    };
    return priorityConfig[priority] || priorityConfig['media'];
}

function getCategoryIcon(category) {
    const icons = {
        'hardware': '🖥️',
        'software': '💾',
        'network': '🌐',
        'email': '📧',
        'phone': '📞',
        'access': '🔐',
        'other': '❓'
    };
    return icons[category] || '❓';
}

function renderMyTickets(ticketsToRender = myTickets) {
    const ticketsList = document.getElementById('myTicketsList');
    
    if (ticketsToRender.length === 0) {
        ticketsList.innerHTML = `
            <div class="p-8 text-center text-gray-500">
                <span class="text-4xl mb-4 block">🎫</span>
                <p>No tienes tickets reportados</p>
                <button class="mt-4 text-blue-600 hover:text-blue-800 font-medium" onclick="showNewTicketForm()">
                    Crear tu primer ticket
                </button>
            </div>
        `;
        return;
    }

    ticketsList.innerHTML = ticketsToRender.map(ticket => {
        const statusConfig = getStatusBadge(ticket.status);
        const priorityConfig = getPriorityBadge(ticket.priority);
        const categoryIcon = getCategoryIcon(ticket.category);
        
        return `
            <div class="ticket-card p-6 border-b border-gray-200 hover:bg-gray-50 cursor-pointer transition-all" onclick="openTicketDetail('${ticket.id}')">
                <div class="flex items-start justify-between">
                    <div class="flex-1">
                        <div class="flex items-center space-x-3 mb-2">
                            <span class="text-lg">${categoryIcon}</span>
                            <span class="text-sm font-mono text-gray-500">${ticket.id}</span>
                            <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${statusConfig.color}">
                                ${statusConfig.icon} ${statusConfig.text}
                            </span>
                            <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${priorityConfig.color}">
                                ${priorityConfig.text}
                            </span>
                        </div>
                        <h3 class="text-lg font-medium text-gray-900 mb-1">${ticket.title}</h3>
                        <p class="text-sm text-gray-600 mb-2">${ticket.description}</p>
                        <div class="flex items-center space-x-4 text-sm text-gray-500">
                            <span>📅 ${ticket.created}</span>
                            <span>👨‍💻 ${ticket.assignedTo}</span>
                            <span>${ticket.messages.length} mensajes</span>
                        </div>
                    </div>
                    <div class="ml-4 text-gray-400">
                        →
                    </div>
                </div>
            </div>
        `;
    }).join('');
}

function filterMyTickets() {
    const searchText = document.getElementById('ticketSearch').value.toLowerCase();
    const statusFilter = document.getElementById('statusFilter').value;
    
    const filteredTickets = myTickets.filter(ticket => {
        const matchesSearch = ticket.title.toLowerCase().includes(searchText) ||
                              ticket.id.toLowerCase().includes(searchText) ||
                              ticket.category.toLowerCase().includes(searchText);
        const matchesStatus = !statusFilter || ticket.status === statusFilter;
        return matchesSearch && matchesStatus;
    });
    renderMyTickets(filteredTickets);
}

function showNewTicketForm(category = '') {
    document.getElementById('newTicketModal').classList.remove('hidden');
    if (category) {
        document.getElementById('ticketCategory').value = category;
    }
}

function closeNewTicketModal() {
    document.getElementById('newTicketModal').classList.add('hidden');
    document.querySelector('#newTicketModal form').reset();
}

// Adjuntos por ticket
function getTicketAttachments(ticket) {
    if (!ticket.attachments || ticket.attachments.length === 0) return '';
    return `
        <div>
            <span class="block text-xs text-gray-500 mb-1">Archivos adjuntos:</span>
            <ul class="space-y-1">
                ${ticket.attachments.map((a, i) => `<li><a href="${a.url}" target="_blank" class="text-blue-600 underline">Archivo ${i + 1}</a></li>`).join('')}
            </ul>
        </div>
    `;
}

function createTicket(event) {
    event.preventDefault();
    const category = document.getElementById('ticketCategory').value;
    const description = document.getElementById('ticketDescription').value;

    // Generar título automático basado en categoría
    const categoryTitles = {
        'hardware': 'Problema con equipo de hardware',
        'software': 'Problema con software/aplicación',
        'network': 'Problema de conectividad',
        'email': 'Problema con correo electrónico',
        'phone': 'Problema de teléfono',
        'access': 'Problema de acceso/permisos',
        'other': 'Consulta general'
    };

    const attachmentInput = document.getElementById('ticketAttachment');
    let attachments = [];
    if (attachmentInput.files.length > 0) {
        for (let i = 0; i < attachmentInput.files.length; i++) {
            const file = attachmentInput.files[i];
            attachments.push({
                name: file.name,
                url: URL.createObjectURL(file)
            });
        }
    }

    const newTicket = {
        id: `TK-${String(Math.floor(Math.random() * 1000) + 100).padStart(3, '0')}`,
        category: category,
        title: categoryTitles[category] || 'Nuevo problema reportado',
        description: description,
        status: 'abierto',
        created: new Date().toISOString().split('T')[0],
        assignedTo: 'Pendiente',
        messages: [
            {
                type: 'user',
                author: currentUser.name,
                time: new Date().toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' }),
                content: description
            }
        ],
        attachments: attachments
    };

    myTickets.unshift(newTicket);
    renderMyTickets();
    closeNewTicketModal();
    showNotification('¡Ticket creado exitosamente! Te contactaremos pronto.', 'success');
}

function openTicketDetail(ticketId) {
    selectedTicket = myTickets.find(t => t.id === ticketId);
    if (!selectedTicket) return;

    document.getElementById('detailTitle').textContent = `${selectedTicket.id} - ${selectedTicket.title}`;
    
    // Cargar información del ticket
    const statusConfig = getStatusBadge(selectedTicket.status);
    const priorityConfig = getPriorityBadge(selectedTicket.priority);
    const categoryIcon = getCategoryIcon(selectedTicket.category);
    
    document.getElementById('ticketInfo').innerHTML = `
        <div class="space-y-3">
            <div>
                <span class="text-2xl">${categoryIcon}</span>
                <h4 class="font-medium text-gray-900 mt-1">${selectedTicket.title}</h4>
            </div>
            <div>
                <span class="text-xs text-gray-500">Estado</span>
                <div class="mt-1">
                    <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${statusConfig.color}">
                        ${statusConfig.icon} ${statusConfig.text}
                    </span>
                </div>
            </div>
            <div>
                <span class="text-xs text-gray-500">Prioridad</span>
                <div class="mt-1">
                    <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${priorityConfig.color}">
                        ${priorityConfig.text}
                    </span>
                </div>
            </div>
            <div>
                <span class="text-xs text-gray-500">Asignado a</span>
                <p class="text-sm text-gray-900 mt-1">${selectedTicket.assignedTo}</p>
            </div>
            <div>
                <span class="text-xs text-gray-500">Creado</span>
                <p class="text-sm text-gray-900 mt-1">${selectedTicket.created}</p>
            </div>
        </div>
    `;

    document.getElementById('ticketAttachments').innerHTML = getTicketAttachments(selectedTicket);

    // Mostrar botones según estado
    document.getElementById('btnReopenTicket').classList.toggle('hidden', selectedTicket.status !== 'cerrado');
    document.getElementById('btnEditTicket').classList.toggle('hidden', selectedTicket.status === 'cerrado' || selectedTicket.status === 'resuelto');
    document.getElementById('btnCancelTicket').classList.toggle('hidden', selectedTicket.status !== 'abierto');
    if (selectedTicket.status === 'en-progreso') {
        document.getElementById('btnCloseTicketContainer').innerHTML = `<button class="bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded-lg w-full mt-2" onclick="showConfirmCloseModal()">Cerrar Ticket</button>`;
    } else {
        document.getElementById('btnCloseTicketContainer').innerHTML = '';
    }

    // Cargar chat
    loadTicketChat();

    // Bloquear input y ocultar editar si está resuelto, cerrado o si es un ticket recién creado (sin mensajes de agente)
    const chatInput = document.getElementById('chatInput');
    const chatBtn = chatInput.nextElementSibling;
    const isRecienCreado = selectedTicket.messages.length === 1 && selectedTicket.messages[0].type === 'user' && selectedTicket.status === 'abierto';
    if (selectedTicket.status === 'resuelto' || selectedTicket.status === 'cerrado' || isRecienCreado) {
        chatInput.disabled = true;
        chatInput.placeholder =
            selectedTicket.status === 'cerrado'
                ? "El ticket está cerrado. No puedes enviar mensajes."
                : selectedTicket.status === 'resuelto'
                    ? "El ticket está resuelto. No puedes enviar mensajes."
                    : "Aún no puedes enviar mensajes. Espera respuesta de soporte.";
        chatBtn.disabled = true;
        chatBtn.classList.add('opacity-50', 'cursor-not-allowed');
        if (selectedTicket.status === 'resuelto' || selectedTicket.status === 'cerrado') {
            document.getElementById('btnEditTicket').classList.add('hidden');
        }
    } else {
        chatInput.disabled = false;
        chatInput.placeholder = "Escribe un mensaje...";
        chatBtn.disabled = false;
        chatBtn.classList.remove('opacity-50', 'cursor-not-allowed');
        // Solo mostrar editar si no está cerrado ni resuelto
        if (selectedTicket.status !== 'cerrado') {
            document.getElementById('btnEditTicket').classList.remove('hidden');
        }
    }

    document.getElementById('ticketDetailModal').classList.remove('hidden');
}

function loadTicketChat() {
    const chatContainer = document.getElementById('ticketChat');
    
    chatContainer.innerHTML = selectedTicket.messages.map(message => `
        <div class="chat-message flex ${message.type === 'user' ? 'justify-end' : 'justify-start'}">
            <div class="max-w-xs lg:max-w-md">
                <div class="flex items-center space-x-2 mb-1">
                    <span class="text-xs text-gray-500">${message.author}</span>
                    <span class="text-xs text-gray-400">${message.time}</span>
                </div>
                <div class="px-3 py-2 rounded-lg text-sm ${
                    message.type === 'user' 
                        ? 'bg-blue-600 text-white' 
                        : 'bg-gray-100 text-gray-900'
                }">
                    ${message.content}
                </div>
            </div>
        </div>
    `).join('');

    // Scroll al final
    chatContainer.scrollTop = chatContainer.scrollHeight;
}

function handleChatKeyPress(event) {
    if (event.key === 'Enter') {
        sendChatMessage();
    }
}

function sendChatMessage() {
    const input = document.getElementById('chatInput');
    const message = input.value.trim();
    
    if (!message || !selectedTicket) return;

    // Agregar mensaje del usuario
    selectedTicket.messages.push({
        type: 'user',
        author: currentUser.name,
        time: new Date().toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' }),
        content: message
    });

    input.value = '';
    loadTicketChat();
    
    // Simular respuesta automática del sistema
    setTimeout(() => {
        selectedTicket.messages.push({
            type: 'system',
            author: 'Sistema',
            time: new Date().toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' }),
            content: 'Tu mensaje ha sido recibido. Un agente te responderá pronto.'
        });
        loadTicketChat();
    }, 1000);
}

function closeTicketDetailModal() {
    document.getElementById('ticketDetailModal').classList.add('hidden');
    selectedTicket = null;
}

function toggleFAQ(id) {
    const content = document.getElementById(`faq-content-${id}`);
    const icon = document.getElementById(`faq-icon-${id}`);
    
    if (content.classList.contains('hidden')) {
        content.classList.remove('hidden');
        icon.textContent = '▲';
    } else {
        content.classList.add('hidden');
        icon.textContent = '▼';
    }
}

// Editar ticket
function showEditTicketModal() {
    if (!selectedTicket) return;
    document.getElementById('editTicketCategory').value = selectedTicket.category;
    document.getElementById('editTicketDescription').value = selectedTicket.description;
    document.getElementById('editTicketModal').classList.remove('hidden');
}
function closeEditTicketModal() {
    document.getElementById('editTicketModal').classList.add('hidden');
}
function updateTicket(event) {
    event.preventDefault();
    selectedTicket.category = document.getElementById('editTicketCategory').value;
    selectedTicket.description = document.getElementById('editTicketDescription').value;
    renderMyTickets();
    closeEditTicketModal();
    showNotification('Ticket actualizado.', 'success');
    openTicketDetail(selectedTicket.id);
}

// Cancelar ticket
function cancelTicket() {
    if (!selectedTicket) return;
    selectedTicket.status = 'cancelado';
    renderMyTickets();
    closeTicketDetailModal();
    showNotification('Ticket cancelado.', 'info');
}

// Reabrir ticket
function reopenTicket() {
    if (!selectedTicket) return;
    selectedTicket.status = 'abierto';
    renderMyTickets();
    closeTicketDetailModal();
    showNotification('Ticket reabierto.', 'success');
}

// Confirmar cierre de ticket
function showConfirmCloseModal() {
    document.getElementById('confirmCloseModal').classList.remove('hidden');
}
function closeConfirmCloseModal() {
    document.getElementById('confirmCloseModal').classList.add('hidden');
}
function confirmCloseTicket() {
    if (!selectedTicket) return;
    selectedTicket.status = 'cerrado';
    renderMyTickets();
    closeConfirmCloseModal();
    closeTicketDetailModal();
    showNotification('Ticket cerrado. Por favor, responde la encuesta.', 'success');
    setTimeout(() => {
        document.getElementById('surveyModal').classList.remove('hidden');
    }, 800);
}

// Encuesta de satisfacción
function closeSurveyModal() {
    document.getElementById('surveyModal').classList.add('hidden');
}
function submitSurvey(event) {
    event.preventDefault();
    closeSurveyModal();
    showNotification('¡Gracias por tu opinión!', 'success');
}

// Perfil
function showProfileModal() {
    document.getElementById('profileInputName').value = currentUser.name;
    document.getElementById('profileInputEmail').value = currentUser.email;
    document.getElementById('profileModal').classList.remove('hidden');
}
function closeProfileModal() {
    document.getElementById('profileModal').classList.add('hidden');
}
function updateProfile(event) {
    event.preventDefault();
    currentUser.name = document.getElementById('profileInputName').value;
    currentUser.email = document.getElementById('profileInputEmail').value;
    document.getElementById('profileName').textContent = currentUser.name;
    closeProfileModal();
    showNotification('Perfil actualizado.', 'success');
}

// Notificaciones instantáneas (simuladas)
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `fixed top-4 right-4 px-6 py-3 rounded-lg text-white z-50 fade-in ${
        type === 'success' ? 'bg-green-500' : 
        type === 'error' ? 'bg-red-500' : 'bg-blue-500'
    }`;
    notification.textContent = message;
    document.body.appendChild(notification);
    setTimeout(() => { notification.remove(); }, 4000);
}

// Inicializar aplicación
document.addEventListener('DOMContentLoaded', function() {
    renderMyTickets();
    // Add event listener for search input
    document.getElementById('ticketSearch').addEventListener('input', filterMyTickets);
});

// Cerrar modales al hacer clic fuera
document.addEventListener('click', function(event) {
    if (event.target.id === 'newTicketModal') {
        closeNewTicketModal();
    }
    if (event.target.id === 'ticketDetailModal') {
        closeTicketDetailModal();
    }
});


//Script por confirmar
function confirmLogout() {
    const confirmExit = confirm("¿Deseas cerrar sesión?");
    if (confirmExit) {
        window.location.href = "/logout"; // cambia la ruta según tu backend
    }
};
