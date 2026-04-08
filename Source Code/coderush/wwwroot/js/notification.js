$(function () {
    var state = {
        currentPage: 1,
        pageSize: 5,
        totalCount: 0,
        unreadCount: 0,
        totalPages: 0,
        pageGroupSize: 5,
        currentPageGroup: 1,
        isOpen: false,
        openMenuId: null,
        searchQuery: '',
        searchDebounceTimer: null
    };

    // ── Toggle dropdown on bell click ──────────────────────────
    $(document).on('click', '#notification-bell', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var $dropdown = $('#notification-dropdown');
        if (state.isOpen) {
            $dropdown.hide();
            state.isOpen = false;
        } else {
            state.currentPage = 1;
            state.currentPageGroup = 1;
            state.searchQuery = '';
            $('#notif-search-bar').hide();
            $('#notif-search-input').val('');
            loadNotifications(1);
            $dropdown.show();
            state.isOpen = true;
        }
    });

    // Close menus / dropdown when clicking outside
    $(document).on('click', function (e) {
        if (state.openMenuId && !$(e.target).closest('.notif-actions').length) {
            $('.notif-actions-menu').hide();
            state.openMenuId = null;
        }
        if (state.isOpen && !$(e.target).closest('.notification-wrapper').length) {
            $('#notification-dropdown').hide();
            state.isOpen = false;
            state.openMenuId = null;
        }
    });

    $(document).on('click', '#notification-dropdown', function (e) {
        e.stopPropagation();
    });

    // ── Search toggle ─────────────────────────────────────────
    $(document).on('click', '#notif-search-btn', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var $bar = $('#notif-search-bar');
        if ($bar.is(':visible')) {
            // Close search and reset
            $bar.hide();
            $('#notif-search-input').val('');
            state.searchQuery = '';
            state.currentPage = 1;
            state.currentPageGroup = 1;
            loadNotifications(1);
        } else {
            $bar.show();
            $('#notif-search-input').focus();
        }
    });

    // Search input with debounce
    $(document).on('input', '#notif-search-input', function () {
        var query = $(this).val().trim();
        clearTimeout(state.searchDebounceTimer);
        state.searchDebounceTimer = setTimeout(function () {
            state.searchQuery = query;
            state.currentPage = 1;
            state.currentPageGroup = 1;
            loadNotifications(1);
        }, 300);
    });

    // Clear search and close search bar
    $(document).on('click', '#notif-search-clear', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $('#notif-search-input').val('');
        $('#notif-search-bar').hide();
        state.searchQuery = '';
        state.currentPage = 1;
        state.currentPageGroup = 1;
        loadNotifications(1);
    });

    // ── Entity name → page URL mapping ─────────────────────────
    var entityUrlMap = {
        'Customer': '/Customer/Index',
        'Vendor': '/Vendor/Index',
        'Product': '/Product/Index',
        'Purchase Order': '/PurchaseOrder/Index',
        'Goods Received Note': '/GoodsReceivedNote/Index',
        'Bill': '/Bill/Index',
        'Payment Voucher': '/PaymentVoucher/Index',
        'Sales Order': '/SalesOrder/Index',
        'Shipment': '/Shipment/Index',
        'Invoice': '/Invoice/Index',
        'Payment Receive': '/PaymentReceive/Index',
        'Bill Type': '/BillType/Index',
        'Branch': '/Branch/Index',
        'Cash Bank': '/CashBank/Index',
        'Currency': '/Currency/Index',
        'Customer Type': '/CustomerType/Index',
        'Invoice Type': '/InvoiceType/Index',
        'Payment Type': '/PaymentType/Index',
        'Product Type': '/ProductType/Index',
        'Sales Type': '/SalesType/Index',
        'Shipment Type': '/ShipmentType/Index',
        'Unit Of Measure': '/UnitOfMeasure/Index',
        'Vendor Type': '/VendorType/Index',
        'Warehouse': '/Warehouse/Index',
        'Purchase Type': '/PurchaseType/Index',
        'User': '/UserRole/Index'
    };

    // ── Click on unread notification → mark read + navigate ────
    $(document).on('click', '.notification-item.notification-clickable .notification-body', function (e) {
        // Don't navigate if the click was on the actions button/menu
        if ($(e.target).closest('.notif-actions').length) return;

        var $item = $(this).closest('.notification-item');
        var id = $item.data('id');
        var url = $item.data('url');

        // Mark as read via API, then navigate
        $.ajax({
            url: '/api/Notification/' + id + '/toggle-read',
            type: 'POST',
            dataType: 'json',
            cache: false,
            success: function (data) {
                updateBadge(data.UnreadCount);
                window.location.href = url;
            },
            error: function () {
                // Navigate anyway even if the API call fails
                window.location.href = url;
            }
        });
    });

    // ── Three-dot menu toggle ──────────────────────────────────
    $(document).on('click', '.notif-actions-btn', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var id = $(this).data('id');
        var $menu = $(this).siblings('.notif-actions-menu');
        $('.notif-actions-menu').not($menu).hide();

        if (state.openMenuId === id) {
            $menu.hide();
            state.openMenuId = null;
        } else {
            $menu.show();
            state.openMenuId = id;
        }
    });

    // ── Mark Read / Unread ─────────────────────────────────────
    $(document).on('click', '.notif-toggle-read', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var id = $(this).data('id');
        $('.notif-actions-menu').hide();
        state.openMenuId = null;

        $.ajax({
            url: '/api/Notification/' + id + '/toggle-read',
            type: 'POST',
            dataType: 'json',
            cache: false,
            success: function (data) {
                state.unreadCount = data.UnreadCount;
                updateBadge(data.UnreadCount);
                var $item = $('.notification-item[data-id="' + id + '"]');
                if (data.IsRead) {
                    $item.addClass('notification-read');
                    $item.removeClass('notification-clickable');
                    $item.find('.notif-link-icon').remove();
                    $item.find('.notif-toggle-read').html('<i class="fa fa-envelope"></i> Mark as unread');
                } else {
                    $item.removeClass('notification-read');
                    // Restore clickable if this item has a navigable URL
                    if ($item.data('url')) {
                        $item.addClass('notification-clickable');
                        var $msg = $item.find('.notification-message');
                        if (!$msg.find('.notif-link-icon').length) {
                            $msg.prepend('<i class="fa fa-external-link notif-link-icon" title="Click to view"></i> ');
                        }
                    }
                    $item.find('.notif-toggle-read').html('<i class="fa fa-envelope-open"></i> Mark as read');
                }
            }
        });
    });

    // ── Delete notification ────────────────────────────────────
    $(document).on('click', '.notif-delete', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var id = $(this).data('id');
        $('.notif-actions-menu').hide();
        state.openMenuId = null;
        $('.notification-item[data-id="' + id + '"]').find('.notif-confirm-overlay').show();
    });

    $(document).on('click', '.notif-confirm-yes', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var id = $(this).data('id');

        $.ajax({
            url: '/api/Notification/' + id,
            type: 'DELETE',
            dataType: 'json',
            cache: false,
            success: function (data) {
                state.unreadCount = data.UnreadCount;
                updateBadge(data.UnreadCount);
                loadNotifications(state.currentPage);
            }
        });
    });

    $(document).on('click', '.notif-confirm-no', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).closest('.notif-confirm-overlay').hide();
    });

    // ── Badge ──────────────────────────────────────────────────
    refreshBadge();

    $(document).ajaxComplete(function (_event, _xhr, settings) {
        if (settings && settings.type &&
            settings.type.toUpperCase() === 'POST' &&
            settings.url &&
            settings.url.indexOf('/api/') !== -1 &&
            settings.url.indexOf('/api/Notification') === -1) {
            setTimeout(refreshBadge, 600);
        }
    });

    function refreshBadge() {
        $.ajax({
            url: '/api/Notification',
            data: { page: 1, pageSize: 1 },
            type: 'GET',
            dataType: 'json',
            cache: false,
            success: function (data) {
                if (data) {
                    state.unreadCount = data.UnreadCount;
                    updateBadge(data.UnreadCount);
                }
            }
        });
    }

    function updateBadge(count) {
        var $badge = $('#notification-badge');
        if (count > 0) {
            $badge.text(count > 99 ? '99+' : count).show();
        } else {
            $badge.hide();
        }
    }

    // ── Load a page of notifications ───────────────────────────
    window.loadNotifications = function (page) {
        var $list = $('#notification-list');
        $list.html('<div class="notification-empty">Loading\u2026</div>');

        var requestData = { page: page, pageSize: state.pageSize };
        if (state.searchQuery) {
            requestData.search = state.searchQuery;
        }

        $.ajax({
            url: '/api/Notification',
            data: requestData,
            type: 'GET',
            dataType: 'json',
            cache: false,
            success: function (data) {
                state.currentPage = data.Page;
                state.totalCount = data.TotalCount;
                state.unreadCount = data.UnreadCount;
                state.totalPages = Math.ceil(data.TotalCount / data.PageSize);
                state.currentPageGroup = Math.ceil(page / state.pageGroupSize);

                updateBadge(data.UnreadCount);
                renderNotifications(data.Items);
                renderPagination();
            },
            error: function () {
                $list.html('<div class="notification-empty">Failed to load notifications.</div>');
            }
        });
    };

    // ── Render items ───────────────────────────────────────────
    function renderNotifications(items) {
        var $list = $('#notification-list');
        $list.empty();

        if (!items || items.length === 0) {
            var emptyMsg = state.searchQuery
                ? 'No notifications matching "' + escapeHtml(state.searchQuery) + '".'
                : 'No notifications yet.';
            $list.html('<div class="notification-empty">' + emptyMsg + '</div>');
            return;
        }

        $.each(items, function (_, item) {
            var time = getTimeAgo(item.CreatedDateTime);
            var readClass = item.IsRead ? ' notification-read' : '';
            var toggleLabel = item.IsRead
                ? '<i class="fa fa-envelope"></i> Mark as unread'
                : '<i class="fa fa-envelope-open"></i> Mark as read';

            // Determine if this notification is clickable
            var targetUrl = item.EntityName ? entityUrlMap[item.EntityName] : null;
            var isClickable = targetUrl && !item.IsRead;
            var clickableClass = isClickable ? ' notification-clickable' : '';
            var dataUrl = isClickable ? ' data-url="' + targetUrl + '"' : '';
            var linkIcon = isClickable
                ? '<i class="fa fa-external-link notif-link-icon" title="Click to view"></i> '
                : '';

            $list.append(
                '<div class="notification-item' + readClass + clickableClass + '" data-id="' + item.NotificationId + '"' + dataUrl + '>' +
                    '<div class="notification-body">' +
                        '<div class="notification-message">' + linkIcon + escapeHtml(item.Message) + '</div>' +
                        '<div class="notification-meta">' +
                            '<span class="notification-time"><i class="fa fa-clock-o"></i> ' + time + '</span>' +
                            '<span class="notif-actions">' +
                                '<button class="notif-actions-btn" data-id="' + item.NotificationId + '" title="Actions">' +
                                    '<i class="fa fa-ellipsis-h"></i>' +
                                '</button>' +
                                '<div class="notif-actions-menu" style="display:none;">' +
                                    '<a href="javascript:void(0);" class="notif-toggle-read" data-id="' + item.NotificationId + '">' + toggleLabel + '</a>' +
                                    '<a href="javascript:void(0);" class="notif-delete" data-id="' + item.NotificationId + '"><i class="fa fa-trash"></i> Delete</a>' +
                                '</div>' +
                            '</span>' +
                        '</div>' +
                    '</div>' +
                    '<div class="notif-confirm-overlay" style="display:none;">' +
                        '<span>Delete this notification?</span>' +
                        '<button class="notif-confirm-yes" data-id="' + item.NotificationId + '">Yes</button>' +
                        '<button class="notif-confirm-no">No</button>' +
                    '</div>' +
                '</div>'
            );
        });
    }

    // ── Pagination ─────────────────────────────────────────────
    function renderPagination() {
        var $footer = $('#notification-footer');
        $footer.empty();

        if (state.totalPages < 1) return;

        var groupSize = state.pageGroupSize;
        var currentGroup = state.currentPageGroup;
        var startPage = (currentGroup - 1) * groupSize + 1;
        var endPage = Math.min(startPage + groupSize - 1, state.totalPages);
        var totalGroups = Math.ceil(state.totalPages / groupSize);
        var html = '<div class="notification-pagination">';

        if (currentGroup > 1) {
            html += '<a href="javascript:void(0);" class="page-btn" data-group="1">First</a>';
            html += '<a href="javascript:void(0);" class="page-btn" data-group="' + (currentGroup - 1) + '">Prev</a>';
        }
        for (var i = startPage; i <= endPage; i++) {
            html += i === state.currentPage
                ? '<span class="page-btn active">' + i + '</span>'
                : '<a href="javascript:void(0);" class="page-btn" data-page="' + i + '">' + i + '</a>';
        }
        if (currentGroup < totalGroups) {
            html += '<a href="javascript:void(0);" class="page-btn" data-group="' + (currentGroup + 1) + '">Next</a>';
            html += '<a href="javascript:void(0);" class="page-btn" data-group="' + totalGroups + '">Last</a>';
        }
        html += '</div>';
        $footer.html(html);
    }

    $(document).on('click', '#notification-footer .page-btn[data-page]', function (e) {
        e.preventDefault();
        loadNotifications(parseInt($(this).data('page'), 10));
    });
    $(document).on('click', '#notification-footer .page-btn[data-group]', function (e) {
        e.preventDefault();
        var g = parseInt($(this).data('group'), 10);
        if (g) loadNotifications((g - 1) * state.pageGroupSize + 1);
    });

    // ── Helpers ────────────────────────────────────────────────
    function getTimeAgo(dateString) {
        var now = new Date(), date = new Date(dateString);
        var s = Math.floor((now - date) / 1000);
        var m = Math.floor(s / 60), h = Math.floor(m / 60), d = Math.floor(h / 24);
        if (s < 60) return 'just now';
        if (m < 60) return m + ' min ago';
        if (h < 24) return h + ' hr ago';
        if (d < 30) return d + ' day' + (d > 1 ? 's' : '') + ' ago';
        return date.toLocaleDateString();
    }

    function escapeHtml(text) {
        if (!text) return '';
        var div = document.createElement('div');
        div.appendChild(document.createTextNode(text));
        return div.innerHTML;
    }
});
