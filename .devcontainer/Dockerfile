# Use the official .NET 8.0 SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Install system dependencies for Avalonia UI (GTK3, X11, etc.)
RUN apt-get update && apt-get install -y \
    libgtk-3-0 \
    libgtk-3-dev \
    libx11-dev \
    libx11-xcb-dev \
    libxcb1-dev \
    libxcb-icccm4-dev \
    libxcb-image0-dev \
    libxcb-keysyms1-dev \
    libxcb-randr0-dev \
    libxcb-render0-dev \
    libxcb-render-util0-dev \
    libxcb-shape0-dev \
    libxcb-shm0-dev \
    libxcb-sync-dev \
    libxcb-util-dev \
    libxcb-xfixes0-dev \
    libxcb-xinerama0-dev \
    libxcb-xkb-dev \
    libxkbcommon-dev \
    libxkbcommon-x11-dev \
    libfontconfig1-dev \
    libfreetype6-dev \
    libgl1-mesa-dev \
    libglib2.0-dev \
    libgdk-pixbuf2.0-dev \
    libpango1.0-dev \
    libcairo2-dev \
    libatk1.0-dev \
    libcanberra-gtk3-module \
    libcanberra-gtk-module \
    xvfb \
    x11vnc \
    fluxbox \
    git \
    openssh-client \
    curl \
    wget \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Create a non-root user for development
ARG USERNAME=vscode
ARG USER_UID=1000
ARG USER_GID=$USER_UID

# Create the user
RUN groupadd --gid $USER_GID $USERNAME \
    && useradd --uid $USER_UID --gid $USER_GID -m $USERNAME \
    && apt-get update \
    && apt-get install -y sudo \
    && echo $USERNAME ALL=\(root\) NOPASSWD:ALL > /etc/sudoers.d/$USERNAME \
    && chmod 0440 /etc/sudoers.d/$USERNAME \
    && rm -rf /var/lib/apt/lists/*

# Install dotnet-ef for the non-root user
RUN su - $USERNAME -c "dotnet tool install --global dotnet-ef --version 8.0.0"

# Add dotnet tools to PATH for the vscode user
ENV PATH="${PATH}:/home/vscode/.dotnet/tools"

# Set working directory
WORKDIR /workspace

# Switch to non-root user
USER $USERNAME

# Set environment variables for Avalonia
ENV DISPLAY=:99
ENV GDK_BACKEND=x11

# Default command
CMD ["/bin/bash"]



