// Datos de ejemplo
let tickets = [
    {
        id: 'TK-045',
        category: 'hardware',
        title: 'Computadora no enciende',
        description: 'La computadora del usuario no enciende desde esta mañana.',
        user: 'Carlos Mendoza',
        department: 'Ventas',
        location: 'Piso 2',
        email: 'carlos.mendoza@empresa.com',
        status: 'en-progreso',
        priority: 'alta',
        created: '2024-01-15',
        lastUpdate: '2024-01-15 10:30',
        unread: true,
        userOnline: true,
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
        id: 'TK-044',
        category: 'software',
        title: 'Excel se cierra automáticamente',
        description: 'Excel se cierra cuando el usuario intenta abrir archivos.',
        user: 'María González',
        department: 'Contabilidad',
        location: 'Piso 3',
        email: 'maria.gonzalez@empresa.com',
        status: 'abierto',
        priority: 'media',
        created: '2024-01-15',
        lastUpdate: '2024-01-15 08:45',
        unread: true,
        userOnline: false,
        messages: [
            {
                type: 'user',
                author: 'María González',
                time: '08:45',
                content: 'Buenos días, tengo un problema con Excel. Se cierra automáticamente cuando trato de abrir cualquier archivo. ¿Pueden ayudarme?'
            }
        ]
    },
    {
        id: 'TK-043',
        category: 'network',
        title: 'Sin acceso a internet',
        description: 'Usuario reporta que no puede acceder a internet desde su computadora.',
        user: 'Roberto Silva',
        department: 'Marketing',
        location: 'Piso 1',
        email: 'roberto.silva@empresa.com',
        status: 'pendiente-usuario',
        priority: 'alta',
        created: '2024-01-14',
        lastUpdate: '2024-01-14 16:20',
        unread: false,
        userOnline: true,
        messages: [
            {
                type: 'user',
                author: 'Roberto Silva',
                time: '15:30',
                content: 'No tengo internet en mi computadora. Los demás equipos del área sí funcionan.'
            },
            {
                type: 'agent',
                author: 'Ana López',
                time: '15:45',
                content: 'Hola Roberto, voy a revisar la configuración de red. ¿Podrías reiniciar tu computadora y confirmarme si aparece algún mensaje de error?'
            },
            {
                type: 'user',
                author: 'Roberto Silva',
                time: '16:20',
                content: 'Ya reinicié pero sigue igual. No aparece ningún mensaje de error específico.'
            }
        ]
    },
    {
        id: 'TK-042',
        category: 'email',
        title: 'No recibe emails',
        description: 'Usuario no está recibiendo correos electrónicos.',
        user: 'Ana Martínez',
        department: 'Recursos Humanos',
        location: 'Piso 4',
        email: 'ana.martinez@empresa.com',
        status: 'resuelto',
        priority: 'media',
        created: '2024-01-14',
        lastUpdate: '2024-01-14 14:30',
        unread: false,
        userOnline: false,
        messages: [
            {
                type: 'user',
                author: 'Ana Martínez',
                time: '10:15',
                content: 'Hola, no estoy recibiendo emails desde ayer. ¿Pueden revisar mi cuenta?'
            },
            {
                type: 'agent',
                author: 'Ana López',
                time: '10:30',
                content: 'Hola Ana, voy a revisar la configuración de tu cuenta de correo. Dame unos minutos.'
            },
            {
                type: 'agent',
                author: 'Ana López',
                time: '11:00',
                content: 'He encontrado el problema. Tu buzón estaba lleno. He liberado espacio y ya deberías recibir correos normalmente.'
            },
            {
                type: 'user',
                author: 'Ana Martínez',
                time: '14:30',
                content: '¡Perfecto! Ya estoy recibiendo emails. Muchas gracias por la ayuda.'
            }
        ]
    },
    {
        id: 'TK-041',
        category: 'access',
        title: 'Solicitud acceso VPN',
        description: 'Usuario solicita acceso VPN para trabajo remoto.',
        user: 'Luis Rodríguez',
        department: 'Desarrollo',
        location: 'Remoto',
        email: 'luis.rodriguez@empresa.com',
        status: 'en-progreso',
        priority: 'baja',
        created: '2024-01-13',
        lastUpdate: '2024-01-13 11:45',
        unread: false,
        userOnline: true,
        messages: [
            {
                type: 'user',
                author: 'Luis Rodríguez',
                time: '09:00',
                content: 'Necesito acceso VPN para trabajar desde casa. ¿Qué documentos necesito enviar?'
            },
            {
                type: 'agent',
                author: 'Ana López',
                time: '09:30',
                content: 'Hola Luis, necesito que envíes una copia de tu identificación y la autorización de tu supervisor para procesar la solicitud de VPN.'
            },
            {
                type: 'user',
                author: 'Luis Rodríguez',
                time: '11:45',
                content: 'Perfecto, ya envié los documentos por email. ¿Cuánto tiempo toma el proceso?'
            }
        ]
    }
];

let selectedTicket = null;
let typingTimeout = null;

// Respuestas rápidas
const quickResponses = {
    saludo: 'Hola {usuario}, gracias por contactar a soporte TI. Voy a revisar tu caso y te ayudo de inmediato.',
    solucion: 'He implementado una solución para tu problema. ¿Podrías probar y confirmarme si ya funciona correctamente?',
    informacion: '¿Podrías proporcionarme más información sobre el problema? Por ejemplo, cuándo comenzó y qué estabas haciendo cuando ocurrió.',
    reiniciar: 'Como primer paso, ¿podrías reiniciar tu computadora y confirmarme si el problema persiste?',
    escalacion: 'Voy a escalar tu caso a un especialista de nivel 2 que te contactará en breve para resolver este problema.',
    despedida: 'Perfecto, me alegra haber podido ayudarte. Si tienes algún otro problema, no dudes en contactarnos. ¡Que tengas un buen día!'
};

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

function getStatusBadge(status) {
    const statusConfig = {
        'abierto': { color: 'bg-blue-100 text-blue-800', text: 'Abierto', icon: '🔵' },
        'en-progreso': { color: 'bg-yellow-100 text-yellow-800', text: 'En Progreso', icon: '🟡' },
        'pendiente-usuario': { color: 'bg-purple-100 text-purple-800', text: 'Pendiente Usuario', icon: '🟣' },
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

function renderTickets(ticketsToRender = tickets) {
    const ticketsList = document.getElementById('ticketsList');
    
    ticketsList.innerHTML = ticketsToRender.map(ticket => {
        const statusConfig = getStatusBadge(ticket.status);
        const priorityConfig = getPriorityBadge(ticket.priority);
        const categoryIcon = getCategoryIcon(ticket.category);
        const isSelected = selectedTicket && selectedTicket.id === ticket.id;
        
        return `
            <div class="ticket-item p-4 border-b border-gray-200 cursor-pointer transition-colors ${isSelected ? 'selected' : ''} ${ticket.unread ? 'unread' : ''}" onclick="selectTicket('${ticket.id}')">
                <div class="flex justify-between items-start mb-2">
                    <div class="flex items-center space-x-2">
                        <span class="text-lg">${categoryIcon}</span>
                        <span class="text-xs font-mono text-gray-500">${ticket.id}</span>
                        ${ticket.unread ? '<div class="w-2 h-2 bg-orange-400 rounded-full notification-dot"></div>' : ''}
                    </div>
                    <div class="flex items-center space-x-1">
                        <div class="w-2 h-2 ${ticket.userOnline ? 'bg-green-400' : 'bg-gray-300'} rounded-full"></div>
                        <span class="text-xs text-gray-500">${ticket.lastUpdate.split(' ')[1]}</span>
                    </div>
                </div>
                <h4 class="font-medium text-gray-900 mb-1 text-sm leading-tight">${ticket.title}</h4>
                <p class="text-xs text-gray-600 mb-2 line-clamp-2">${ticket.description}</p>
                <div class="flex items-center justify-between mb-2">
                    <div class="flex items-center space-x-1">
                        <span class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${statusConfig.color}">${statusConfig.text}</span>
                        <span class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${priorityConfig.color}">${priorityConfig.text}</span>
                    </div>
                </div>
                <div class="flex items-center justify-between text-xs text-gray-500">
                    <span>👤 ${ticket.user}</span>
                    <span>💬 ${ticket.messages.length}</span>
                </div>
            </div>
        `;
    }).join('');

    updateStats();
}

function updateStats() {
    const openCount = tickets.filter(t => t.status === 'abierto').length;
    const inProgressCount = tickets.filter(t => t.status === 'en-progreso').length;
    const urgentCount = tickets.filter(t => t.priority === 'urgente').length;

    document.getElementById('openTickets').textContent = openCount;
    document.getElementById('inProgressTickets').textContent = inProgressCount;
    document.getElementById('urgentTickets').textContent = urgentCount;
}

function selectTicket(ticketId) {
    selectedTicket = tickets.find(t => t.id === ticketId);
    if (!selectedTicket) return;

    // Marcar como leído
    selectedTicket.unread = false;

    // Actualizar UI
    document.getElementById('noTicketSelected').classList.add('hidden');
    document.getElementById('ticketView').classList.remove('hidden');

    // Actualizar información del ticket
    document.getElementById('ticketCategoryIcon').textContent = getCategoryIcon(selectedTicket.category);
    document.getElementById('ticketId').textContent = selectedTicket.id;
    document.getElementById('ticketTitle').textContent = selectedTicket.title;
    document.getElementById('ticketUser').textContent = `👤 ${selectedTicket.user}`;
    document.getElementById('ticketDepartment').textContent = `🏢 ${selectedTicket.department}`;
    document.getElementById('ticketLocation').textContent = `📍 ${selectedTicket.location}`;
    document.getElementById('ticketDate').textContent = `📅 ${selectedTicket.created}`;

    // Actualizar estado online del usuario
    const onlineStatus = document.getElementById('userOnlineStatus');
    if (selectedTicket.userOnline) {
        onlineStatus.innerHTML = '<div class="w-2 h-2 bg-green-400 rounded-full"></div><span>Usuario en línea</span>';
        onlineStatus.className = 'flex items-center space-x-1 text-xs text-green-600';
    } else {
        onlineStatus.innerHTML = '<div class="w-2 h-2 bg-gray-300 rounded-full"></div><span>Usuario desconectado</span>';
        onlineStatus.className = 'flex items-center space-x-1 text-xs text-gray-500';
    }

    // Actualizar badges
    const statusConfig = getStatusBadge(selectedTicket.status);
    const priorityConfig = getPriorityBadge(selectedTicket.priority);
    
    document.getElementById('ticketStatus').className = `inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${statusConfig.color}`;
    document.getElementById('ticketStatus').textContent = statusConfig.text;
    
    document.getElementById('ticketPriority').className = `inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${priorityConfig.color}`;
    document.getElementById('ticketPriority').textContent = priorityConfig.text;

    // Actualizar select de estado
    document.getElementById('ticketStatusSelect').value = selectedTicket.status;

    // Cargar mensajes del chat
    loadChatMessages();

    // Re-renderizar lista para mostrar selección
    renderTickets();
}

function loadChatMessages() {
    const chatContainer = document.getElementById('chatMessages');
    
    chatContainer.innerHTML = selectedTicket.messages.map(message => `
        <div class="chat-message flex ${message.type === 'agent' ? 'justify-end' : 'justify-start'}">
            <div class="max-w-xs lg:max-w-md">
                <div class="flex items-center space-x-2 mb-1 ${message.type === 'agent' ? 'justify-end' : 'justify-start'}">
                    <span class="text-xs text-gray-500">${message.author}</span>
                    <span class="text-xs text-gray-400">${message.time}</span>
                </div>
                <div class="px-3 py-2 rounded-lg text-sm ${
                    message.type === 'agent' 
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

function filterTickets() {
    const searchTerm = document.getElementById('searchTickets').value.toLowerCase();
    const statusFilter = document.getElementById('statusFilter').value;
    const priorityFilter = document.getElementById('priorityFilter').value;
    const categoryFilter = document.getElementById('categoryFilter').value;

    const filteredTickets = tickets.filter(ticket => {
        const matchesSearch = ticket.title.toLowerCase().includes(searchTerm) || 
                            ticket.user.toLowerCase().includes(searchTerm) ||
                            ticket.id.toLowerCase().includes(searchTerm);
        const matchesStatus = !statusFilter || ticket.status === statusFilter;
        const matchesPriority = !priorityFilter || ticket.priority === priorityFilter;
        const matchesCategory = !categoryFilter || ticket.category === categoryFilter;

        return matchesSearch && matchesStatus && matchesPriority && matchesCategory;
    });

    renderTickets(filteredTickets);
}

function updateTicketStatus() {
    if (!selectedTicket) return;
    
    const newStatus = document.getElementById('ticketStatusSelect').value;
    selectedTicket.status = newStatus;
    
    // Actualizar badge
    const statusConfig = getStatusBadge(newStatus);
    document.getElementById('ticketStatus').className = `inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${statusConfig.color}`;
    document.getElementById('ticketStatus').textContent = statusConfig.text;
    
    renderTickets();
    showNotification('Estado del ticket actualizado', 'success');
}

function handleChatKeyPress(event) {
    if (event.ctrlKey && event.key === 'Enter') {
        sendMessage();
    }
}

function handleTyping() {
    const input = document.getElementById('chatInput');
    const charCount = document.getElementById('charCount');
    charCount.textContent = `${input.value.length}/500`;
    
    if (input.value.length > 500) {
        input.value = input.value.substring(0, 500);
        charCount.textContent = '500/500';
    }
}

function insertQuickResponse(type) {
    const response = quickResponses[type];
    const input = document.getElementById('chatInput');
    const userName = selectedTicket ? selectedTicket.user : 'Usuario';
    
    const processedResponse = response.replace('{usuario}', userName);
    input.value = processedResponse;
    input.focus();
    handleTyping();
}

function sendMessage() {
    const input = document.getElementById('chatInput');
    const message = input.value.trim();
    
    if (!message || !selectedTicket) return;

    // Agregar mensaje del agente
    selectedTicket.messages.push({
        type: 'agent',
        author: 'Ana López',
        time: new Date().toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' }),
        content: message
    });

    // Actualizar timestamp del ticket
    selectedTicket.lastUpdate = new Date().toISOString().split('T')[0] + ' ' + new Date().toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' });

    input.value = '';
    handleTyping();
    loadChatMessages();
    renderTickets();
    
    showNotification('Mensaje enviado', 'success');

    // Simular respuesta del usuario (solo para demo)
    if (selectedTicket.userOnline && Math.random() > 0.7) {
        setTimeout(() => {
            simulateUserResponse();
        }, 2000 + Math.random() * 3000);
    }
}

function simulateUserResponse() {
    if (!selectedTicket) return;

    const responses = [
        'Perfecto, voy a probar eso ahora.',
        'Sí, eso funcionó. Muchas gracias.',
        'Hmm, sigue sin funcionar.',
        'Ok, dame un momento para revisar.',
        'Excelente, ya está resuelto.',
        'No estoy seguro de cómo hacer eso.'
    ];

    const randomResponse = responses[Math.floor(Math.random() * responses.length)];
    
    selectedTicket.messages.push({
        type: 'user',
        author: selectedTicket.user,
        time: new Date().toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' }),
        content: randomResponse
    });

    selectedTicket.unread = true;
    loadChatMessages();
    renderTickets();
    
    showNotification(`Nuevo mensaje de ${selectedTicket.user}`, 'info');
}

function attachFile() {
    showNotification('Función de adjuntar archivos en desarrollo', 'info');
}

function showTicketInfo() {
    if (!selectedTicket) return;

    const statusConfig = getStatusBadge(selectedTicket.status);
    const priorityConfig = getPriorityBadge(selectedTicket.priority);
    const categoryIcon = getCategoryIcon(selectedTicket.category);

    document.getElementById('ticketInfoContent').innerHTML = `
        <div class="space-y-4">
            <div class="flex items-center space-x-3">
                <span class="text-2xl">${categoryIcon}</span>
                <div>
                    <h4 class="font-medium text-gray-900">${selectedTicket.title}</h4>
                    <p class="text-sm text-gray-600">${selectedTicket.id}</p>
                </div>
            </div>
            
            <div class="grid grid-cols-2 gap-4 text-sm">
                <div>
                    <span class="text-gray-500">Usuario:</span>
                    <p class="font-medium">${selectedTicket.user}</p>
                </div>
                <div>
                    <span class="text-gray-500">Email:</span>
                    <p class="font-medium">${selectedTicket.email}</p>
                </div>
                <div>
                    <span class="text-gray-500">Departamento:</span>
                    <p class="font-medium">${selectedTicket.department}</p>
                </div>
                <div>
                    <span class="text-gray-500">Ubicación:</span>
                    <p class="font-medium">${selectedTicket.location}</p>
                </div>
                <div>
                    <span class="text-gray-500">Estado:</span>
                    <span class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${statusConfig.color}">
                        ${statusConfig.text}
                    </span>
                </div>
                <div>
                    <span class="text-gray-500">Prioridad:</span>
                    <span class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${priorityConfig.color}">
                        ${priorityConfig.text}
                    </span>
                </div>
                <div>
                    <span class="text-gray-500">Creado:</span>
                    <p class="font-medium">${selectedTicket.created}</p>
                </div>
                <div>
                    <span class="text-gray-500">Última actualización:</span>
                    <p class="font-medium">${selectedTicket.lastUpdate}</p>
                </div>
            </div>
            
            <div>
                <span class="text-gray-500">Descripción:</span>
                <p class="mt-1 text-sm text-gray-900">${selectedTicket.description}</p>
            </div>
        </div>
    `;

    document.getElementById('ticketInfoModal').classList.remove('hidden');
}

function closeTicketInfoModal() {
    document.getElementById('ticketInfoModal').classList.add('hidden');
}

function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `fixed top-4 right-4 px-4 py-2 rounded-lg text-white z-50 fade-in ${
        type === 'success' ? 'bg-green-500' : 
        type === 'error' ? 'bg-red-500' : 'bg-blue-500'
    }`;
    notification.textContent = message;
    
    document.body.appendChild(notification);
    
    setTimeout(() => {
        notification.remove();
    }, 3000);
}

// Inicializar aplicación
document.addEventListener('DOMContentLoaded', function() {
    renderTickets();
});

// Cerrar modal al hacer clic fuera
document.addEventListener('click', function(event) {
    if (event.target.id === 'ticketInfoModal') {
        closeTicketInfoModal();
    }
});

// Simular nuevos tickets ocasionalmente
setInterval(() => {
    if (Math.random() > 0.95) { // 5% de probabilidad cada 10 segundos
        const newTicketId = `TK-${String(Math.floor(Math.random() * 1000) + 100).padStart(3, '0')}`;
        const categories = ['hardware', 'software', 'network', 'email'];
        const users = ['Pedro López', 'Carmen Ruiz', 'Diego Morales', 'Sofía Castro'];
        const departments = ['Ventas', 'Marketing', 'Contabilidad', 'Desarrollo'];
        
        const randomCategory = categories[Math.floor(Math.random() * categories.length)];
        const randomUser = users[Math.floor(Math.random() * users.length)];
        const randomDepartment = departments[Math.floor(Math.random() * departments.length)];
        
        const newTicket = {
            id: newTicketId,
            category: randomCategory,
            title: `Nuevo problema de ${randomCategory}`,
            description: `Usuario reporta problema con ${randomCategory}`,
            user: randomUser,
            department: randomDepartment,
            location: 'Piso 2',
            email: `${randomUser.toLowerCase().replace(' ', '.')}@empresa.com`,
            status: 'abierto',
            priority: 'media',
            created: new Date().toISOString().split('T')[0],
            lastUpdate: new Date().toISOString().split('T')[0] + ' ' + new Date().toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' }),
            unread: true,
            userOnline: true,
            messages: [
                {
                    type: 'user',
                    author: randomUser,
                    time: new Date().toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' }),
                    content: `Hola, tengo un problema con ${randomCategory}. ¿Pueden ayudarme?`
                }
            ]
        };
        
        tickets.unshift(newTicket);
        renderTickets();
        showNotification(`Nuevo ticket de ${randomUser}`, 'info');
    }
}, 10000);
