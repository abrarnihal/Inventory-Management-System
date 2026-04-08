(function () {
    'use strict';

    $(document).ready(function () {
        var conversationHistory = [];
        var currentConversationId = null;
        var conversations = [];
        var isWaiting = false;
        var pendingConversations = {};
        var pollingTimers = {};
        var pendingFiles = [];
        var MAX_FILES = 5;
        var selectedConversationStorageKey = 'chatbot-current-conversation-id';

        var panel = document.getElementById('chatbot-panel');
        var overlay = document.getElementById('chatbot-overlay');
        var messagesContainer = document.getElementById('chatbot-messages');
        var input = document.getElementById('chatbot-input');
        var sendBtn = document.getElementById('chatbot-send-btn');
        var stopBtn = document.getElementById('chatbot-stop-btn');
        var attachBtn = document.getElementById('chatbot-attach-btn');
        var fileInput = document.getElementById('chatbot-file-input');
        var fileIndicator = document.getElementById('chatbot-file-indicator');
        var fileList = document.getElementById('chatbot-file-list');
        var removeAllBtn = document.getElementById('chatbot-file-remove-all');
        var sidebar = document.getElementById('chatbot-sidebar');
        var sidebarOverlay = document.getElementById('chatbot-sidebar-overlay');
        var sidebarToggle = document.getElementById('chatbot-sidebar-toggle');
        var conversationListEl = document.getElementById('chatbot-conversation-list');
        var newChatBtn = document.getElementById('chatbot-new-chat-btn');

        // Sidebar helpers
        function openSidebar() {
            sidebar.classList.add('open');
            sidebarOverlay.classList.add('active');
        }

        function closeSidebar() {
            sidebar.classList.remove('open');
            sidebarOverlay.classList.remove('active');
        }

        function toggleSidebar() {
            if (sidebar.classList.contains('open')) {
                closeSidebar();
            } else {
                openSidebar();
            }
        }

        // Send / Stop button helpers
        function showStopBtn() {
            sendBtn.style.display = 'none';
            stopBtn.style.display = 'flex';
        }

        function showSendBtn() {
            stopBtn.style.display = 'none';
            sendBtn.style.display = 'flex';
            sendBtn.disabled = false;
        }

        function persistCurrentConversation() {
            if (currentConversationId) {
                localStorage.setItem(selectedConversationStorageKey, currentConversationId.toString());
            } else {
                localStorage.removeItem(selectedConversationStorageKey);
            }
        }

        function getPersistedConversationId() {
            var storedValue = localStorage.getItem(selectedConversationStorageKey);
            if (!storedValue) return null;

            var parsedValue = parseInt(storedValue, 10);
            return isNaN(parsedValue) ? null : parsedValue;
        }

        function clearPolling(conversationId) {
            if (pollingTimers[conversationId]) {
                clearInterval(pollingTimers[conversationId]);
                delete pollingTimers[conversationId];
            }
        }

        function markConversationPending(conversationId, isPending) {
            if (!conversationId) return;

            if (isPending) {
                pendingConversations[conversationId] = true;
            } else {
                delete pendingConversations[conversationId];
                clearPolling(conversationId);
            }
        }

        function finishPendingConversation(conversationId, shouldReloadHistory) {
            markConversationPending(conversationId, false);

            if (conversationId === currentConversationId) {
                hideTyping();
                isWaiting = false;
                showSendBtn();
                if (shouldReloadHistory) {
                    loadChatHistory();
                }
                input.focus();
            }

            loadConversations();
        }

        function startPollingConversation(conversationId) {
            if (!conversationId || pollingTimers[conversationId]) return;

            pollingTimers[conversationId] = setInterval(function () {
                $.ajax({
                    url: '/api/ChatBot/PendingStatus?conversationId=' + conversationId,
                    type: 'GET',
                    dataType: 'json',
                    success: function (data) {
                        if (!data.Success || !data.IsPending) {
                            finishPendingConversation(conversationId, true);
                        }
                    },
                    error: function () {
                        finishPendingConversation(conversationId, true);
                    }
                });
            }, 1500);
        }

        function stopMessage() {
            if (!currentConversationId || !pendingConversations[currentConversationId]) return;

            $.ajax({
                url: '/api/ChatBot/StopResponse?conversationId=' + currentConversationId,
                type: 'POST',
                dataType: 'json',
                success: function (data) {
                    if (data.Success) {
                        finishPendingConversation(currentConversationId, true);
                    }
                }
            });
        }

        // Open / Close
        window.openChatBot = function () {
            panel.classList.add('open');
            overlay.classList.add('active');
            input.focus();
        };

        window.closeChatBot = function () {
            panel.classList.remove('open');
            overlay.classList.remove('active');
            closeSidebar();
        };

        overlay.addEventListener('click', function () {
            closeChatBot();
        });

        // Close sidebar when clicking outside it
        sidebarOverlay.addEventListener('click', function () {
            closeSidebar();
        });

        // Close sidebar when interacting with the input area
        document.querySelector('.chatbot-input-area').addEventListener('click', function () {
            if (sidebar.classList.contains('open')) {
                closeSidebar();
            }
        });

        // Close sidebar when clicking anywhere on the header except the sidebar toggle
        document.querySelector('.chatbot-header').addEventListener('click', function (e) {
            if (sidebar.classList.contains('open') && !sidebarToggle.contains(e.target)) {
                closeSidebar();
            }
        });

        // Sidebar toggle
        sidebarToggle.addEventListener('click', function () {
            toggleSidebar();
        });

        // New chat
        newChatBtn.addEventListener('click', function () {
            createNewConversation();
        });

        // --- Conversation management ---

        function loadConversations(callback) {
            $.ajax({
                url: '/api/ChatBot/Conversations',
                type: 'GET',
                dataType: 'json',
                success: function (data) {
                    conversations = data.Conversations || [];

                    for (var i = 0; i < conversations.length; i++) {
                        if (conversations[i].IsPending) {
                            pendingConversations[conversations[i].ChatConversationId] = true;
                        } else {
                            delete pendingConversations[conversations[i].ChatConversationId];
                            clearPolling(conversations[i].ChatConversationId);
                        }
                    }

                    renderConversationList();
                    if (callback) callback();
                },
                error: function () {
                    conversations = [];
                    renderConversationList();
                    if (callback) callback();
                }
            });
        }

        function renderConversationList() {
            var pinnedList = document.getElementById('chatbot-pinned-list');
            var unpinnedList = document.getElementById('chatbot-unpinned-list');
            var pinnedSection = document.getElementById('chatbot-pinned-section');
            var unpinnedLabel = document.getElementById('chatbot-unpinned-label');

            pinnedList.innerHTML = '';
            unpinnedList.innerHTML = '';

            var hasPinned = false;
            var unpinnedConvs = [];

            for (var i = 0; i < conversations.length; i++) {
                var conv = conversations[i];
                if (conv.IsPinned) {
                    hasPinned = true;
                    pinnedList.appendChild(createConversationItem(conv));
                } else {
                    unpinnedConvs.push(conv);
                }
            }

            // Sort unpinned by CreatedAt descending (newest first)
            unpinnedConvs.sort(function (a, b) {
                return new Date(b.CreatedAt) - new Date(a.CreatedAt);
            });

            // Group by date and render with DD/MM/YYYY headers
            var lastDateKey = '';
            for (var j = 0; j < unpinnedConvs.length; j++) {
                var dateKey = formatDateKey(unpinnedConvs[j].CreatedAt);
                if (dateKey !== lastDateKey) {
                    var header = document.createElement('div');
                    header.className = 'chatbot-date-header';
                    header.textContent = dateKey;
                    unpinnedList.appendChild(header);
                    lastDateKey = dateKey;
                }
                unpinnedList.appendChild(createConversationItem(unpinnedConvs[j]));
            }

            pinnedSection.style.display = hasPinned ? '' : 'none';
            unpinnedLabel.style.display = hasPinned ? '' : 'none';
        }

        function formatDateKey(dateStr) {
            var d = new Date(dateStr);
            var day = ('0' + d.getDate()).slice(-2);
            var month = ('0' + (d.getMonth() + 1)).slice(-2);
            var year = d.getFullYear();
            return day + '/' + month + '/' + year;
        }

        function createConversationItem(conv) {
            var item = document.createElement('div');
            item.className = 'chatbot-conversation-item' +
                (conv.ChatConversationId === currentConversationId ? ' active' : '');
            item.setAttribute('data-id', conv.ChatConversationId);

            var icon = document.createElement('i');
            icon.className = 'fa fa-comment-o';
            item.appendChild(icon);

            var titleSpan = document.createElement('span');
            titleSpan.className = 'chatbot-conv-title';
            titleSpan.textContent = conv.Title || 'New Chat';
            item.appendChild(titleSpan);

            // Three-dot menu wrapper
            var menuWrapper = document.createElement('div');
            menuWrapper.className = 'chatbot-conv-menu-wrapper';

            var menuBtn = document.createElement('button');
            menuBtn.className = 'chatbot-conv-menu-btn';
            menuBtn.title = 'Options';
            menuBtn.innerHTML = '<i class="fa fa-ellipsis-v"></i>';
            menuWrapper.appendChild(menuBtn);

            var dropdown = document.createElement('div');
            dropdown.className = 'chatbot-conv-dropdown';

            var renameOption = document.createElement('button');
            renameOption.className = 'chatbot-conv-dropdown-item';
            renameOption.innerHTML = '<i class="fa fa-pencil"></i> Rename';
            renameOption.addEventListener('click', (function (id, itemEl) {
                return function (e) {
                    e.stopPropagation();
                    closeAllConvMenus();
                    showRenameField(id, itemEl);
                };
            })(conv.ChatConversationId, item));
            dropdown.appendChild(renameOption);

            var pinOption = document.createElement('button');
            pinOption.className = 'chatbot-conv-dropdown-item';
            pinOption.innerHTML = conv.IsPinned
                ? '<i class="fa fa-thumb-tack"></i> Unpin'
                : '<i class="fa fa-thumb-tack"></i> Pin';
            pinOption.addEventListener('click', (function (id) {
                return function (e) {
                    e.stopPropagation();
                    closeAllConvMenus();
                    togglePin(id);
                };
            })(conv.ChatConversationId));
            dropdown.appendChild(pinOption);

            menuWrapper.appendChild(dropdown);

            // Attach click handler after dropdown is created so the IIFE captures the real element
            menuBtn.addEventListener('click', (function (wrapper, dropdownEl, btnEl) {
                return function (e) {
                    e.stopPropagation();
                    closeAllConvMenus();
                    wrapper.classList.toggle('open');
                    if (wrapper.classList.contains('open')) {
                        var rect = btnEl.getBoundingClientRect();
                        dropdownEl.style.top = (rect.bottom + 2) + 'px';
                        dropdownEl.style.right = '';
                        dropdownEl.style.left = Math.max(0, rect.right - dropdownEl.offsetWidth) + 'px';
                    }
                };
            })(menuWrapper, dropdown, menuBtn));

            item.appendChild(menuWrapper);

            item.addEventListener('click', (function (id) {
                return function () {
                    switchConversation(id);
                    closeSidebar();
                };
            })(conv.ChatConversationId));

            return item;
        }

        function closeAllConvMenus() {
            var openMenus = document.querySelectorAll('.chatbot-conv-menu-wrapper.open');
            for (var i = 0; i < openMenus.length; i++) {
                openMenus[i].classList.remove('open');
            }
        }

        // Close menus when clicking elsewhere inside the sidebar
        document.addEventListener('click', function () {
            closeAllConvMenus();
            cancelActiveRename();
        });

        function showRenameField(conversationId, itemEl) {
            // Cancel any other active rename fields first
            cancelActiveRename();

            var titleSpan = itemEl.querySelector('.chatbot-conv-title');
            var menuWrapper = itemEl.querySelector('.chatbot-conv-menu-wrapper');
            if (!titleSpan) return;

            var currentTitle = titleSpan.textContent;
            titleSpan.style.display = 'none';
            if (menuWrapper) menuWrapper.style.display = 'none';

            var renameContainer = document.createElement('div');
            renameContainer.className = 'chatbot-rename-container';

            var renameInput = document.createElement('input');
            renameInput.type = 'text';
            renameInput.className = 'chatbot-rename-input';
            renameInput.value = currentTitle;
            renameInput.maxLength = 50;

            var saveBtn = document.createElement('button');
            saveBtn.className = 'chatbot-rename-save';
            saveBtn.title = 'Save';
            saveBtn.innerHTML = '<i class="fa fa-check"></i>';

            var cancelBtn = document.createElement('button');
            cancelBtn.className = 'chatbot-rename-cancel';
            cancelBtn.title = 'Cancel';
            cancelBtn.innerHTML = '<i class="fa fa-times"></i>';

            renameContainer.appendChild(renameInput);
            renameContainer.appendChild(saveBtn);
            renameContainer.appendChild(cancelBtn);

            // Insert after titleSpan
            titleSpan.parentNode.insertBefore(renameContainer, titleSpan.nextSibling);

            renameInput.focus();
            renameInput.select();

            // Prevent item click from switching conversation
            renameContainer.addEventListener('click', function (e) {
                e.stopPropagation();
            });

            function doSave() {
                var newTitle = renameInput.value.trim();
                if (newTitle && newTitle !== currentTitle) {
                    renameConversation(conversationId, newTitle);
                }
                cleanupRename();
            }

            function cleanupRename() {
                titleSpan.style.display = '';
                if (menuWrapper) menuWrapper.style.display = '';
                if (renameContainer.parentNode) {
                    renameContainer.parentNode.removeChild(renameContainer);
                }
            }

            saveBtn.addEventListener('click', function (e) {
                e.stopPropagation();
                doSave();
            });

            cancelBtn.addEventListener('click', function (e) {
                e.stopPropagation();
                cleanupRename();
            });

            renameInput.addEventListener('keydown', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    doSave();
                } else if (e.key === 'Escape') {
                    e.preventDefault();
                    cleanupRename();
                }
            });
        }

        function cancelActiveRename() {
            var activeRenames = document.querySelectorAll('.chatbot-rename-container');
            for (var i = 0; i < activeRenames.length; i++) {
                var container = activeRenames[i];
                var item = container.closest('.chatbot-conversation-item');
                if (item) {
                    var titleSpan = item.querySelector('.chatbot-conv-title');
                    var menuWrapper = item.querySelector('.chatbot-conv-menu-wrapper');
                    if (titleSpan) titleSpan.style.display = '';
                    if (menuWrapper) menuWrapper.style.display = '';
                }
                if (container.parentNode) {
                    container.parentNode.removeChild(container);
                }
            }
        }

        function renameConversation(conversationId, newTitle) {
            $.ajax({
                url: '/api/ChatBot/RenameConversation',
                type: 'POST',
                contentType: 'application/json',
                dataType: 'json',
                data: JSON.stringify({ ConversationId: conversationId, Title: newTitle }),
                success: function (data) {
                    if (data.Success) {
                        for (var i = 0; i < conversations.length; i++) {
                            if (conversations[i].ChatConversationId === conversationId) {
                                conversations[i].Title = data.Title;
                                break;
                            }
                        }
                        renderConversationList();
                    }
                }
            });
        }

        function togglePin(conversationId) {
            $.ajax({
                url: '/api/ChatBot/TogglePin?conversationId=' + conversationId,
                type: 'POST',
                dataType: 'json',
                success: function (data) {
                    if (data.Success) {
                        for (var i = 0; i < conversations.length; i++) {
                            if (conversations[i].ChatConversationId === conversationId) {
                                conversations[i].IsPinned = data.IsPinned;
                                break;
                            }
                        }
                        renderConversationList();
                    }
                }
            });
        }

        function createNewConversation(callback) {
            $.ajax({
                url: '/api/ChatBot/CreateConversation',
                type: 'POST',
                dataType: 'json',
                success: function (data) {
                    if (data.Success) {
                        currentConversationId = data.ChatConversationId;
                        persistCurrentConversation();
                        conversationHistory = [];
                        isWaiting = false;
                        markConversationPending(currentConversationId, false);
                        showSendBtn();
                        showWelcomeMessage();
                        loadConversations();
                        closeSidebar();
                        if (callback) callback();
                    } else {
                        appendMessage('assistant', 'Failed to create a new conversation. Please try again.');
                    }
                },
                error: function () {
                    appendMessage('assistant', 'Failed to create a new conversation. Please try again.');
                }
            });
        }

        function switchConversation(conversationId) {
            if (conversationId === currentConversationId) return;
            currentConversationId = conversationId;
            persistCurrentConversation();
            conversationHistory = [];
            messagesContainer.innerHTML = '';

            // Restore waiting state for the target conversation
            if (pendingConversations[conversationId]) {
                isWaiting = true;
                showStopBtn();
            } else {
                isWaiting = false;
                showSendBtn();
            }

            loadChatHistory();
            renderConversationList();
        }

        function appendWelcomeMessage() {
            var welcomeDiv = document.createElement('div');
            welcomeDiv.className = 'chatbot-msg assistant';
            welcomeDiv.innerHTML =
                '<p>Hello! I\'m your Inventory Management AI Assistant. I can help you with:</p>' +
                '<ul><li>Viewing products, customers, vendors, and orders</li>' +
                '<li>Creating new records</li>' +
                '<li>Searching and deleting data</li>' +
                '<li>Getting inventory summaries</li>' +
                '<li><strong>Analyzing uploaded files</strong> (.txt, .md, .docx, .xlsx)</li></ul>' +
                '<p>How can I help you today?</p>';
            messagesContainer.appendChild(welcomeDiv);
        }

        function showWelcomeMessage() {
            messagesContainer.innerHTML = '';
            appendWelcomeMessage();
        }

        // --- Persistence helpers ---

        function saveChatMessage(role, text, fileNames, conversationId) {
            var convId = conversationId || currentConversationId;
            if (!convId) return;
            $.ajax({
                url: '/api/ChatBot/SaveMessage',
                type: 'POST',
                contentType: 'application/json',
                dataType: 'json',
                data: JSON.stringify({
                    ConversationId: convId,
                    Role: role,
                    Content: text,
                    FileNames: fileNames && fileNames.length > 0 ? fileNames : null
                }),
                success: function (data) {
                    if (data.Success && data.Title) {
                        updateConversationTitle(convId, data.Title);
                    }
                }
            });
        }

        function updateConversationTitle(convId, title) {
            for (var i = 0; i < conversations.length; i++) {
                if (conversations[i].ChatConversationId === convId) {
                    conversations[i].Title = title;
                    break;
                }
            }
            renderConversationList();
        }

        function loadChatHistory() {
            if (!currentConversationId) {
                showWelcomeMessage();
                return;
            }
            $.ajax({
                url: '/api/ChatBot/History?conversationId=' + currentConversationId,
                type: 'GET',
                dataType: 'json',
                success: function (data) {
                    messagesContainer.innerHTML = '';
                    conversationHistory = [];
                    appendWelcomeMessage();
                    if (data.Messages && data.Messages.length > 0) {
                        for (var i = 0; i < data.Messages.length; i++) {
                            var msg = data.Messages[i];
                            appendMessage(msg.Role, msg.Text, msg.FileNames);
                            conversationHistory.push({ Role: msg.Role, Content: msg.Text });
                        }
                    }
                    // Re-show typing indicator if this conversation still has a pending request
                    if (pendingConversations[currentConversationId]) {
                        isWaiting = true;
                        showStopBtn();
                        showTyping();
                        startPollingConversation(currentConversationId);
                    }
                },
                error: function () {
                    showWelcomeMessage();
                }
            });
        }

        // --- Confirm dialog for deleting conversation ---

        var confirmDialog = document.getElementById('chatbot-confirm-dialog');
        var confirmYesBtn = document.getElementById('chatbot-confirm-yes');
        var confirmNoBtn = document.getElementById('chatbot-confirm-no');

        window.confirmClearChat = function () {
            confirmDialog.style.display = 'flex';
        };

        function hideConfirmDialog() {
            confirmDialog.style.display = 'none';
        }

        confirmYesBtn.addEventListener('click', function () {
            hideConfirmDialog();
            if (!currentConversationId) return;

            var deletingId = currentConversationId;
            $.ajax({
                url: '/api/ChatBot/DeleteConversation?conversationId=' + deletingId,
                type: 'DELETE',
                dataType: 'json',
                success: function (data) {
                    if (data.Success) {
                        currentConversationId = null;
                        persistCurrentConversation();
                        conversationHistory = [];
                        loadConversations(function () {
                            if (conversations.length > 0) {
                                switchConversation(conversations[0].ChatConversationId);
                            } else {
                                createNewConversation();
                            }
                        });
                    } else {
                        appendMessage('assistant', 'Failed to delete the conversation. Please try again.');
                    }
                },
                error: function () {
                    appendMessage('assistant', 'Failed to delete the conversation. Please try again.');
                }
            });
        });

        confirmNoBtn.addEventListener('click', function () {
            hideConfirmDialog();
        });

        // --- File attachment ---

        $(document).on('change', '#chatbot-file-input', function () {
            var files = this.files;
            if (!files || files.length === 0) return;

            for (var i = 0; i < files.length; i++) {
                if (pendingFiles.length >= MAX_FILES) break;
                var isDuplicate = pendingFiles.some(function (f) {
                    return f.name === files[i].name && f.size === files[i].size;
                });
                if (isDuplicate) continue;
                pendingFiles.push(files[i]);
            }

            fileInput.value = '';
            renderFileList();
        });

        $(removeAllBtn).on('click', function () {
            clearAllFiles();
        });

        $(fileList).on('click', '.chatbot-file-chip button', function () {
            var idx = parseInt($(this).data('idx'), 10);
            if (!isNaN(idx) && idx >= 0 && idx < pendingFiles.length) {
                pendingFiles.splice(idx, 1);
                renderFileList();
            }
        });

        function renderFileList() {
            fileList.innerHTML = '';
            for (var i = 0; i < pendingFiles.length; i++) {
                var chip = document.createElement('span');
                chip.className = 'chatbot-file-chip';
                chip.innerHTML =
                    '<i class="fa fa-file-o"></i>' +
                    '<span>' + escapeHtml(pendingFiles[i].name) + '</span>' +
                    '<button type="button" data-idx="' + i + '" title="Remove">&times;</button>';
                fileList.appendChild(chip);
            }

            if (pendingFiles.length > 0) {
                fileIndicator.style.display = 'flex';
                attachBtn.classList.add('has-file');
            } else {
                fileIndicator.style.display = 'none';
                attachBtn.classList.remove('has-file');
            }
        }

        function clearAllFiles() {
            pendingFiles = [];
            fileInput.value = '';
            renderFileList();
        }

        // --- Send message ---

        function sendMessage() {
            var text = input.value.trim();
            if (!text || isWaiting || !currentConversationId) return;

            var hasFiles = pendingFiles.length > 0;
            var fileNames = hasFiles ? pendingFiles.map(function (f) { return f.name; }) : [];

            // Capture conversation context at send time
            var sentConversationId = currentConversationId;

            appendMessage('user', text, fileNames);
            conversationHistory.push({ Role: 'user', Content: text });
            input.value = '';

            isWaiting = true;
            markConversationPending(sentConversationId, true);
            showStopBtn();
            showTyping();

            if (hasFiles) {
                var formData = new FormData();
                for (var i = 0; i < pendingFiles.length; i++) {
                    formData.append('files', pendingFiles[i]);
                }
                formData.append('conversationId', sentConversationId);
                formData.append('message', text);
                formData.append('historyJson', JSON.stringify(conversationHistory.slice(0, -1)));

                clearAllFiles();

                $.ajax({
                    url: '/api/ChatBot/BeginResponseWithFiles',
                    type: 'POST',
                    data: formData,
                    processData: false,
                    contentType: false,
                    success: function (data) { handleStartResponse(data, sentConversationId); },
                    error: function () { handleStartError(sentConversationId); }
                });
            } else {
                var payload = {
                    ConversationId: sentConversationId,
                    Message: text,
                    History: conversationHistory.slice(0, -1)
                };

                $.ajax({
                    url: '/api/ChatBot/BeginResponse',
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify(payload),
                    success: function (data) { handleStartResponse(data, sentConversationId); },
                    error: function () { handleStartError(sentConversationId); }
                });
            }
        }

        function handleStartResponse(data, sentConversationId) {
            if (!pendingConversations[sentConversationId]) return;

            if (!data || !data.Success || !data.IsPending) {
                handleStartError(sentConversationId, data && data.Message ? data.Message : null);
                return;
            }

            if (data.Title) {
                updateConversationTitle(sentConversationId, data.Title);
            }

            startPollingConversation(sentConversationId);
        }

        function handleStartError(sentConversationId, customMessage) {
            if (!pendingConversations[sentConversationId]) return;

            finishPendingConversation(sentConversationId, true);

            if (sentConversationId === currentConversationId) {
                var errorMsg = customMessage || 'Sorry, something went wrong. Please try again.';
                appendMessage('assistant', errorMsg);
                input.focus();
            }
        }

        sendBtn.addEventListener('click', sendMessage);
        stopBtn.addEventListener('click', stopMessage);
        input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                sendMessage();
            }
        });

        // --- Rendering helpers ---

        function appendMessage(role, text, attachedFileNames) {
            var div = document.createElement('div');
            div.className = 'chatbot-msg ' + role;

            if (role === 'user' && attachedFileNames && attachedFileNames.length > 0) {
                for (var f = 0; f < attachedFileNames.length; f++) {
                    var badge = document.createElement('div');
                    badge.className = 'file-badge';
                    badge.innerHTML = '<i class="fa fa-file-o"></i>' + escapeHtml(attachedFileNames[f]);
                    div.appendChild(badge);
                }
            }

            if (role === 'assistant') {
                div.innerHTML += renderMarkdown(text);
            } else {
                var span = document.createElement('span');
                span.textContent = text;
                div.appendChild(span);
            }

            messagesContainer.appendChild(div);
            messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }

        function escapeHtml(str) {
            var d = document.createElement('div');
            d.textContent = str;
            return d.innerHTML;
        }

        function showTyping() {
            var div = document.createElement('div');
            div.className = 'chatbot-typing';
            div.id = 'chatbot-typing-indicator';
            div.innerHTML = '<span class="dot"></span><span class="dot"></span><span class="dot"></span>';
            messagesContainer.appendChild(div);
            messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }

        function hideTyping() {
            var el = document.getElementById('chatbot-typing-indicator');
            if (el) el.remove();
        }

        // --- Simple markdown renderer ---

        function renderMarkdown(text) {
            if (!text) return '';

            var html = text
                .replace(/&/g, '&amp;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;');

            html = renderTables(html);

            html = html.replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>');
            html = html.replace(/\*(.+?)\*/g, '<em>$1</em>');
            html = html.replace(/`([^`]+)`/g, '<code>$1</code>');

            html = html.replace(/^[\-\*] (.+)$/gm, '<li>$1</li>');
            html = html.replace(/(<li>.*<\/li>\n?)+/g, '<ul>$&</ul>');

            html = html.replace(/^\d+\. (.+)$/gm, '<li>$1</li>');

            html = html.replace(/^### (.+)$/gm, '<strong>$1</strong>');
            html = html.replace(/^## (.+)$/gm, '<strong>$1</strong>');
            html = html.replace(/^# (.+)$/gm, '<strong>$1</strong>');

            html = html.replace(/\n\n/g, '</p><p>');
            html = '<p>' + html + '</p>';
            html = html.replace(/\n/g, '<br>');
            html = html.replace(/<p><\/p>/g, '');
            html = html.replace(/<p>\s*<br>\s*<\/p>/g, '');

            return html;
        }

        function renderTables(text) {
            var lines = text.split('\n');
            var result = [];
            var tableLines = [];
            var inTable = false;

            for (var i = 0; i < lines.length; i++) {
                var line = lines[i].trim();
                if (line.indexOf('|') !== -1 && line.length > 2) {
                    tableLines.push(line);
                    inTable = true;
                } else {
                    if (inTable && tableLines.length >= 2) {
                        result.push(buildTable(tableLines));
                        tableLines = [];
                        inTable = false;
                    } else if (inTable) {
                        for (var j = 0; j < tableLines.length; j++) {
                            result.push(tableLines[j]);
                        }
                        tableLines = [];
                        inTable = false;
                    }
                    result.push(lines[i]);
                }
            }
            if (inTable && tableLines.length >= 2) {
                result.push(buildTable(tableLines));
            } else {
                for (var k = 0; k < tableLines.length; k++) {
                    result.push(tableLines[k]);
                }
            }
            return result.join('\n');
        }

        function buildTable(lines) {
            var html = '<table>';
            var headerProcessed = false;

            for (var i = 0; i < lines.length; i++) {
                var line = lines[i].trim();
                if (/^\|?[\s\-:]+(\|[\s\-:]+)+\|?$/.test(line)) continue;

                var cells = line.split('|').filter(function (c, idx, arr) {
                    return idx > 0 && idx < arr.length - 1;
                });
                if (cells.length === 0) {
                    cells = line.split('|').filter(function (c) { return c.trim() !== ''; });
                }

                if (!headerProcessed) {
                    html += '<thead><tr>';
                    for (var h = 0; h < cells.length; h++) {
                        html += '<th>' + cells[h].trim() + '</th>';
                    }
                    html += '</tr></thead><tbody>';
                    headerProcessed = true;
                } else {
                    html += '<tr>';
                    for (var c = 0; c < cells.length; c++) {
                        html += '<td>' + cells[c].trim() + '</td>';
                    }
                    html += '</tr>';
                }
            }

            html += '</tbody></table>';
            return html;
        }

        // --- Initialize: load conversations, select most recent or create new ---
        loadConversations(function () {
            var persistedConversationId = getPersistedConversationId();
            var matchedConversation = null;

            if (persistedConversationId) {
                for (var i = 0; i < conversations.length; i++) {
                    if (conversations[i].ChatConversationId === persistedConversationId) {
                        matchedConversation = conversations[i];
                        break;
                    }
                }
            }

            if (matchedConversation) {
                currentConversationId = matchedConversation.ChatConversationId;
            } else if (conversations.length > 0) {
                currentConversationId = conversations[0].ChatConversationId;
            }

            if (currentConversationId) {
                persistCurrentConversation();
                loadChatHistory();
                renderConversationList();
            } else {
                createNewConversation();
            }
        });

    });
})();