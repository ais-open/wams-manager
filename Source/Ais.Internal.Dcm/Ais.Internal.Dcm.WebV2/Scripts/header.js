document.write(
    "<div class='navbar'>" +
        "<div class='navbar-inner'>" +
            "<div class='container'>" +
                "<button type='button' class='btn btn-navbar' data-toggle='collapse' data-target='.nav-collapse'>" +
                    "<span class='icon-bar'></span>" +
                    "<span class='icon-bar'></span>" +
                    "<span class='icon-bar'></span>" +
                "</button>" +
                "<div class='brand'><span>AIS WAMS</span><span>Management Portal</span></div>" +
                "<div class='nav-collapse collapse'>" +
                    "<div class='pull-right'>" +
                        "<ul class='nav pull-right'>" +
                            "<li class='dropdown'><a href='#' class='dropdown-toggle' data-toggle='dropdown'>Welcome, <span id='loggedUser'></span><b class='caret'></b></a>" +
                                 "<ul class='dropdown-menu'>" +
                                    "<li><a href='./ChangePassword.html'><i class='icon-lock' style='font-size: 17px;'></i> Change Password</a></li>" +
                                    "<li id='createUserLink'><a href='../UserPages/CreateUser.html'><i id='I1' class='icon-plus-sign-alt'></i> Create User</a></li>" +
                                    "<li id='manageUserLink'><a href='../UserPages/ManageUser.html' style=''><i class='icon-group'></i> Manage Users</a></li><li class='divider'></li>" +
                                    "<li><a id='signout' href='#' style=''><i class='icon-signout' style='font-size: 17px;'></i> Logout</a></li>" +
                                "</ul>" +
                            "</li>" +
                         "</ul>" +
                    "</div>" +
                "</div>" +
            "</div>" +
                                 "<ul class='nav wams-nav'>" +
                        "<li id='homeLink'><a href='./Home.html'>Home</a></li>" +
                        "<li id='mediaLink' ><a href='./MediaService.html'>Media Services</a></li>" +
                        "<li id='encodingLink'><a href='./Encoding.html'>Encoding</a></li>" +
                       // "<li ><a href='./About.html'>About</a></li>" +
                    "</ul>" +
        "</div>" +
    "</div>");